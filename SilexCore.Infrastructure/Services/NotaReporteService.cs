using System.Globalization;
using SilexCore.Domain.Interfaces.Infraestructura.Persistence;
using SilexCore.Domain.Interfaces.Infraestructura.Services;

namespace SilexCore.Infrastructure.Services;

// Arma los datos de la representación gráfica (PDF/HTML) de una nota de
// crédito y la manda a generar vía ReportingService (plantilla "Nota").
// tbl_nota no trae cliente/receptor/detalle de líneas propios -- se heredan
// de la factura referenciada (mismo criterio que SP_ObtenerNotaXml usa para
// armar el XML real), por eso se reusan IFacturaRepository/IClienteRepository
// tal cual los usa FacturaReporteService.
public class NotaReporteService(
    INotaRepository notaRepo,
    IFacturaRepository facturaRepo,
    IClienteRepository clienteRepo,
    IUsuarioRepository usuarioRepo,
    IImagenReporteService imagenes,
    IReporteService reportes) : INotaReporteService
{
    public async Task<(byte[] Contenido, string ContentType)?> GenerarReporteAsync(int idNota, string formato = "pdf")
    {
        var nota = await notaRepo.ObtenerNotaPorIdAsync(idNota);
        if (nota is null) return null;

        var detalle = await facturaRepo.ObtenerDetalleFacturaPorIdAsync(nota.IdFactura);
        var cliente = await clienteRepo.ObtenerClientePorIdAsync(nota.IdCliente);
        var entidad = await usuarioRepo.ObtenerEntidadReporteAsync(nota.IdEntidad);
        var configuracion = await usuarioRepo.ObtenerConfiguracionDocumentoAsync();

        var logoDataUri = await imagenes.ObtenerComoDataUriAsync(entidad?.LogoArchivo);
        var fondoDataUri = await imagenes.ObtenerComoDataUriAsync(entidad?.FondoArchivo);

        var lineas = new List<object>();
        decimal subtotalGeneral = 0, descuentoGeneral = 0, gravadoGeneral = 0, exoneradoGeneral = 0;

        // Mismo cálculo simplificado que FacturaReporteService: la exoneración
        // mueve el impuesto completo de la línea a "Exonerado", sin prorratear.
        foreach (var d in detalle)
        {
            var lineaSubtotal = d.Cantidad * d.Precio;
            var lineaDescuento = lineaSubtotal * d.PorcentajeDescuento / 100m;
            var baseImponible = lineaSubtotal - lineaDescuento;
            var impuestoLinea = baseImponible * d.ValorTarifa / 100m;

            var gravado = d.IdExoneracion > 0 ? 0m : impuestoLinea;
            var exonerado = d.IdExoneracion > 0 ? impuestoLinea : 0m;

            subtotalGeneral += lineaSubtotal;
            descuentoGeneral += lineaDescuento;
            gravadoGeneral += gravado;
            exoneradoGeneral += exonerado;

            lineas.Add(new
            {
                codigo = d.Codigo,
                unidad_medida = d.UnidadMedida,
                detalle = d.Opcion,
                cantidad = Formato(d.Cantidad),
                precio_unitario = Formato(d.Precio),
                subtotal = Formato(lineaSubtotal),
                descuento = Formato(lineaDescuento),
                gravado = Formato(gravado),
                exonerado = Formato(exonerado)
            });
        }

        var ventaNeta = subtotalGeneral - descuentoGeneral;
        var impuestoNeto = gravadoGeneral;
        var total = ventaNeta + impuestoNeto;

        var datos = new
        {
            negocio = new
            {
                nombre_comercial = entidad?.NombreComercial,
                razon_social = entidad?.RazonSocial,
                identificacion = entidad?.Identificacion,
                codigo_pais = entidad?.CodigoPais,
                telefono = entidad?.Telefono,
                correo = entidad?.Correo,
                direccion_completa = $"{entidad?.Provincia}, {entidad?.Canton}, {entidad?.Distrito}, {entidad?.Direccion}",
                logo_data_uri = logoDataUri,
                fondo_data_uri = fondoDataUri,
                cuenta_leyenda = entidad?.CuentaLeyenda,
                cuenta_representante = entidad?.CuentaRepresentante,
                cuenta_corriente = entidad?.CuentaCorriente,
                cuenta_cliente = entidad?.CuentaCliente,
                cuenta_iban = entidad?.CuentaIban
            },
            cliente = new
            {
                identificacion = cliente?.Identificacion,
                nombre = cliente?.Nombre,
                telefono = cliente?.Telefono,
                correo = cliente?.Correo
            },
            documento = new
            {
                consecutivo = nota.Consecutivo,
                clave = nota.Clave,
                fecha = nota.Fecha,
                condicion_venta = nota.CondicionVenta,
                medio_pago = nota.MedioPago,
                moneda = nota.Moneda,
                moneda_valor = Formato(nota.MonedaValor),
                detalle = nota.Razon,
                accion = nota.Accion,
                razon = nota.Razon,
                referencia_consecutivo = nota.FacturaConsecutivo,
                referencia_clave = nota.FacturaClave
            },
            lineas,
            totales = new
            {
                subtotal = Formato(subtotalGeneral),
                descuento = Formato(descuentoGeneral),
                venta_neta = Formato(ventaNeta),
                gravado = Formato(gravadoGeneral),
                exonerado = Formato(exoneradoGeneral),
                impuesto_neto = Formato(impuestoNeto),
                total = Formato(total)
            },
            monto_en_letras = NumeroALetrasHelper.Convertir(total, MonedaTexto(nota.Moneda)),
            configuracion = new
            {
                version = configuracion?.VersionComprobante ?? "4.4",
                texto_autorizacion = configuracion?.TextoAutorizacion
            },
            pagina = 1
        };

        return await reportes.GenerarReporteAsync("Nota", datos, formato);
    }

    private static string Formato(decimal valor) => valor.ToString("N2", CultureInfo.InvariantCulture);

    private static string MonedaTexto(string? moneda) => (moneda ?? "Colones").ToUpperInvariant();
}
