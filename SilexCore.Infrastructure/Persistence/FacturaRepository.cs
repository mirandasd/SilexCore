using Karin.Persistence.Dapper.DbContext;
using Karin.Persistence.Dapper.Interfaces;
using SilexCore.Domain.Constants;
using SilexCore.Domain.Dtos;
using SilexCore.Domain.Interfaces.Infraestructura.Persistence;

namespace SilexCore.Infrastructure.Persistence;

public class FacturaRepository(ISqlExecutor sqlExecutor): IFacturaRepository
{
    public Task<IEnumerable<DocumentosPorNegocioResponse>> ObtenerFacturasPorNegocioAsync(int idEntidad)
        => sqlExecutor.QueryAsync<DocumentosPorNegocioResponse>("ObtenerFacturasPorNegocio", new { IdEntidad = idEntidad }, connectionName: ConnectionNames.Default);

    public Task<IEnumerable<FacturaPorConsecutivoResponse>> ObtenerFacturaPorConsecutivoYNegocioAsync(int idEntidad, string consecutivo)
        => sqlExecutor.QueryAsync<FacturaPorConsecutivoResponse>("ObtenerFacturaPorConsecutivo", new { IdEntidad = idEntidad, Consecutivo = consecutivo }, connectionName: ConnectionNames.Default);

    public Task<IEnumerable<FacturaReferenciaResponse>> BuscarFacturaParaNotaAsync(int idEntidad, string consecutivo)
        => sqlExecutor.QueryAsync<FacturaReferenciaResponse>("ObtenerFacturaPorConsecutivoParaNota", new { IdEntidad = idEntidad, Consecutivo = consecutivo }, connectionName: ConnectionNames.Default);

    public Task<FacturaResponse?> ObtenerFacturaPorIdAsync(int idFactura)
        => sqlExecutor.QueryFirstOrDefaultAsync<FacturaResponse?>("ObtenerFacturaPorId", new { IdFactura = idFactura }, connectionName: ConnectionNames.Default);

    public Task<IEnumerable<DetalleFacturaResponse>> ObtenerDetalleFacturaPorIdAsync(int idFactura)
        => sqlExecutor.QueryAsync<DetalleFacturaResponse>("ObtenerDetallePorIdFactura", new { IdFactura = idFactura }, connectionName: ConnectionNames.Default);

    public Task<HistoricoFacturaDto?> ObtenerHistoricoFacturaPorIdAsync(int idFactura)
        => sqlExecutor.QueryFirstOrDefaultAsync<HistoricoFacturaDto?>("ObtenerHistoricoFacturaPorId", new { IdFactura = idFactura }, connectionName: ConnectionNames.Default);

    public Task<(int RowsAffected, string Mensaje)> AgregarFacturaAsync(AgregarFacturaRequest factura)
        => sqlExecutor.ExecuteSpWithOutputAsync("AgregarFactura", factura, connectionName: ConnectionNames.Default);

    // Todo en una sola transacción física (conexión de sw_procesos, con llamadas
    // calificadas por esquema hacia sw_cliente/sw_administracion -- las 4 BDs viven
    // en el mismo servidor MySQL, así que una sola conexión puede tocar las tres y
    // un ROLLBACK deshace todo, cliente incluido). Ver TransactionalSqlExecutor:
    // el parámetro connectionName se ignora dentro de la transacción, por eso el
    // nombre del procedimiento se califica con el esquema en vez de usarlo.
    public Task<RegistrarFacturaResult> RegistrarFacturaCompletaAsync(RegistrarFacturaRequest request)
        => sqlExecutor.ExecuteInTransactionAsync(async tx =>
        {
            // 1. Cliente: si no viene con id ya existente, se crea/completa (típico de
            //    un cliente recién encontrado en Hacienda, sin registro local todavía).
            var idCliente = request.Cliente.IdCliente;
            if (idCliente <= 0)
            {
                var (rowsCliente, idClienteStr) = await tx.ExecuteSpWithOutputAsync("sw_cliente.AgregarClienteMinimoExtendido", new
                {
                    IdCliente = 0,
                    request.Cliente.IdTipoIdentificacion,
                    Identificacion = request.Cliente.Identificacion ?? string.Empty,
                    Nombre = request.Cliente.Nombre ?? string.Empty,
                    Correo = request.Cliente.Correo ?? string.Empty,
                    RegimenDescripcion = request.Cliente.RegimenDescripcion ?? string.Empty,
                    request.Cliente.RegimenCodigo,
                    ActividadDescripcion = request.Cliente.ActividadDescripcion ?? string.Empty,
                    activadadTipo = request.Cliente.ActividadTipo ?? string.Empty, // el parámetro del SP tiene ese typo de origen
                    ActividadCodigo = request.Cliente.ActividadCodigo ?? string.Empty
                });

                if (rowsCliente <= 0 || !int.TryParse(idClienteStr, out idCliente))
                    throw new InvalidOperationException($"No se pudo registrar el cliente: {idClienteStr}");
            }

            // 2. Consecutivo vigente para este negocio + tipo de comprobante.
            var consecutivo = await tx.QueryFirstOrDefaultAsync<string?>("sw_administracion.ObtenerConsecutivoPorNegocioYTipo", new
            {
                request.IdEntidad,
                Tipo = request.IdTipoComprobante
            });

            if (string.IsNullOrWhiteSpace(consecutivo))
                throw new InvalidOperationException("No se encontró un consecutivo configurado para este negocio y tipo de comprobante.");

            // 3. Factura.
            var (rowsFactura, idFacturaStr) = await tx.ExecuteSpWithOutputAsync("AgregarFactura", new
            {
                Consecutivo = consecutivo,
                request.IdEntidad,
                IdCliente = idCliente,
                request.IdTipoComprobante,
                request.IdCondicionVenta,
                PlazoCredito = request.PlazoCredito ?? string.Empty, // tbl_factura.plazo_credito es NOT NULL
                request.IdMoneda,
                request.MonedaValor,
                request.IdMedioPago,
                request.PagoCliente,
                request.VueltoCliente,
                request.Detalle,
                request.CodigoActividad
            });

            if (rowsFactura <= 0 || !int.TryParse(idFacturaStr, out var idFactura))
                throw new InvalidOperationException($"No se pudo registrar la factura: {idFacturaStr}");

            // 4. Detalle -- no todas las líneas llevan exoneración.
            foreach (var linea in request.Lineas)
            {
                var idExoneracion = 0;
                if (linea.Exoneracion is not null)
                {
                    var (rowsExo, idExoneracionStr) = await tx.ExecuteSpWithOutputAsync("AgregarExoneracionCompleta", new
                    {
                        identificacion = linea.Exoneracion.Identificacion,
                        numeroDocumento = linea.Exoneracion.NumeroDocumento,
                        codigoInstitucion = linea.Exoneracion.CodigoInstitucion,
                        fechaEmision = linea.Exoneracion.FechaEmision,
                        porcentajeExoneracion = linea.Exoneracion.Porcentaje,
                        tipoAutorizacion = linea.Exoneracion.TipoAutorizacion,
                        fechaVencimiento = linea.Exoneracion.FechaVencimiento,
                        autorizacion = linea.Exoneracion.Autorizacion,
                        ano = linea.Exoneracion.Anno,
                        tipoDocumentoCodigo = linea.Exoneracion.TipoDocumentoCodigo,
                        codigoProyectoCFIA = linea.Exoneracion.CodigoProyectoCFIA,
                        articulo = linea.Exoneracion.Articulo,
                        inciso = linea.Exoneracion.Inciso,
                        poseeCabys = linea.Exoneracion.PoseeCabys
                    });

                    // AgregarExoneracionCompleta es get-or-create: cuando la exoneración ya
                    // existe no hace ningún INSERT, así que rowsExo llega en 0 aunque el OUT
                    // sí traiga el id real -- no sirve como señal de éxito, solo el id parseado.
                    if (!int.TryParse(idExoneracionStr, out idExoneracion) || idExoneracion <= 0)
                        throw new InvalidOperationException($"No se pudo registrar la exoneración: {idExoneracionStr}");

                    if (linea.Exoneracion.PoseeCabys && linea.Exoneracion.Cabys is { Count: > 0 })
                    {
                        foreach (var item in linea.Exoneracion.Cabys)
                        {
                            await tx.ExecuteAsync("AgregarExoneracionCabys", new
                            {
                                idExoneracion,
                                codigoCabys = item.CodigoCabys,
                                detalleCabys = item.DetalleCabys
                            });
                        }
                    }
                }

                // Se arma el objeto a mano (no se reusa AgregarDetalleFacturaRequest tal
                // cual): trae la propiedad Exoneracion anidada, que no existe como
                // parámetro del SP, y MySql.Data exige que cada parámetro enviado
                // corresponda a uno real del procedimiento.
                var (rowsDetalle, msjDetalle) = await tx.ExecuteSpWithOutputAsync("AgregarDetalleFactura", new
                {
                    IdFactura = idFactura,
                    IdExoneracion = idExoneracion,
                    linea.IdOpcionVenta,
                    linea.IdCodigoProducto,
                    linea.IdUnidadMedida,
                    // tbl_factura_detalle.medida_comercial/detalle/naturaleza_descuento son NOT NULL
                    medidaComercial = linea.MedidaComercial ?? string.Empty,
                    codigoCabys = linea.CodigoCabys,
                    cantidad = linea.Cantidad,
                    precio = linea.Precio,
                    detalle = linea.Detalle ?? string.Empty,
                    porcentajeDescuento = linea.PorcentajeDescuento,
                    naturalezaDescuento = linea.NaturalezaDescuento ?? string.Empty,
                    linea.IdTipoImpuesto,
                    linea.IdTipoTarifa
                });

                if (rowsDetalle <= 0)
                    throw new InvalidOperationException($"No se pudo registrar el detalle de la factura: {msjDetalle}");
            }

            // 5. Se actualiza el consecutivo al final -- solo si todo lo anterior salió bien.
            var (rowsConsecutivo, msjConsecutivo) = await tx.ExecuteSpWithOutputAsync("sw_administracion.ActualizarConsecutivoPorRegistro", new
            {
                request.IdEntidad,
                Tipo = request.IdTipoComprobante
            });

            if (rowsConsecutivo <= 0)
                throw new InvalidOperationException($"No se pudo actualizar el consecutivo: {msjConsecutivo}");

            return new RegistrarFacturaResult
            {
                IdFactura = idFactura,
                IdCliente = idCliente,
                Consecutivo = consecutivo,
                Mensaje = "Factura registrada correctamente"
            };
        }, connectionName: ConnectionNames.Default);

    public Task<(int RowsAffected, string Mensaje)> AgregarDetalleFacturaAsync(AgregarDetalleFacturaRequest detalleFactura)
        => sqlExecutor.ExecuteSpWithOutputAsync("AgregarDetalleFactura", detalleFactura, connectionName: ConnectionNames.Default);

    // AgregarExoneracion (el SP viejo) se deja intacto por si algo más lo usa -- sus
    // nombres de parámetro no calzan con ExoneracionRequest (era de un modelo previo,
    // sin CABYS). Este método usa AgregarExoneracionCompleta, que además resuelve
    // tipoDocumento/institución por código y hace get-or-create (permite "recargar"
    // una exoneración ya registrada sin duplicarla).
    public Task<(int RowsAffected, string Mensaje)> AgregarExoneracionAsync(ExoneracionRequest exoneracion)
        => sqlExecutor.ExecuteInTransactionAsync(async tx =>
        {
            var (rows, idExoneracionStr) = await tx.ExecuteSpWithOutputAsync("AgregarExoneracionCompleta", new
            {
                identificacion = exoneracion.Identificacion,
                numeroDocumento = exoneracion.NumeroDocumento,
                codigoInstitucion = exoneracion.CodigoInstitucion,
                fechaEmision = exoneracion.FechaEmision,
                porcentajeExoneracion = exoneracion.Porcentaje,
                tipoAutorizacion = exoneracion.TipoAutorizacion,
                fechaVencimiento = exoneracion.FechaVencimiento,
                autorizacion = exoneracion.Autorizacion,
                ano = exoneracion.Anno,
                tipoDocumentoCodigo = exoneracion.TipoDocumentoCodigo,
                codigoProyectoCFIA = exoneracion.CodigoProyectoCFIA,
                articulo = exoneracion.Articulo,
                inciso = exoneracion.Inciso,
                poseeCabys = exoneracion.PoseeCabys
            });

            // Mismo caso que en RegistrarFacturaCompletaAsync: get-or-create no reporta
            // filas afectadas cuando reusa una exoneración existente, así que el éxito lo
            // marca el id parseado, no "rows".
            if (!int.TryParse(idExoneracionStr, out var idExoneracion) || idExoneracion <= 0)
                return (0, idExoneracionStr);

            if (exoneracion.PoseeCabys && exoneracion.Cabys is { Count: > 0 })
            {
                foreach (var item in exoneracion.Cabys)
                {
                    await tx.ExecuteAsync("AgregarExoneracionCabys", new
                    {
                        idExoneracion,
                        codigoCabys = item.CodigoCabys,
                        detalleCabys = item.DetalleCabys
                    });
                }
            }

            return (1, idExoneracionStr);
        }, connectionName: ConnectionNames.Default);

    public Task<IEnumerable<ExoneracionResponse>> ObtenerExoneracionPorIdClienteAsync(int idCliente)
        => sqlExecutor.QueryAsync<ExoneracionResponse>("ObtenerExoneracionPorIdCliente", new { IdCliente = idCliente }, connectionName: ConnectionNames.Default);

    public Task<IEnumerable<ExoneracionCabysItem>> ObtenerExoneracionCabysPorIdAsync(int idExoneracion)
        => sqlExecutor.QueryAsync<ExoneracionCabysItem>("ObtenerExoneracionCabysPorId", new { IdExoneracion = idExoneracion }, connectionName: ConnectionNames.Default);

    public Task<(int RowsAffected, string Mensaje)> GuardarHistoricoFacturaAsync(
        int idFactura, string clave, string pathXml, string pathPdf, string pathRespuesta, int estadoEnvio, int estadoHacienda)
        => sqlExecutor.ExecuteSpWithOutputAsync("SP_AgregarHistoricoFactura", new
        {
            idDocumento = idFactura,
            Fecha = DateTime.Now,
            Clave = clave,
            PathDocumentoXml = pathXml,
            PathDocumentoPdf = pathPdf, // vacío por ahora -- el PDF se genera en un paso aparte, todavía no conectado
            PathDocumentoRespuesta = pathRespuesta,
            EstadoEnvio = estadoEnvio,
            EstadoHacienda = estadoHacienda
        }, connectionName: ConnectionNames.Default);

    //public async Task<int> RegistrarPedidoAsync(PedidoDto dto)
    //{
    //    return await _sqlExecutor.ExecuteInTransactionAsync(async tx =>
    //    {
    //await tx.ExecuteAsync("AgregarClienteMinimo", new { dto.IdCliente, dto.Monto }); lo primero que se hace es el proceso del cliente minimo porque se ocupa el id del cliente para la factura
    //        var (rows, msg) = await tx.ExecuteSpWithOutputAsync("AgregarFactura", dto.Pedido);
    //        if (rows == 0)
    //            throw new InvalidOperationException(msg); // dispara rollback automático
    //await tx.ExecuteAsync("AgregarExoneracion", dto.Detalle); - Se agrega exoneracion solo si viene, podria ser que un detalle no lleve exoneracion y se ocupa el id para registrarlo en el detalle
    //        await tx.ExecuteAsync("AgregarDetalleFactura", dto.Detalle);
    //        

    //        return dto.Pedido.Id;
    //    });
    //}
}
