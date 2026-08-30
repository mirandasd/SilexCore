using SilexCore.Domain.Dtos;

namespace SilexCore.Domain.Interfaces.Infraestructura.Persistence;

public interface IFacturaRepository
{
    Task<(int RowsAffected, string Mensaje)> AgregarDetalleFacturaAsync(AgregarDetalleFacturaRequest detalleFactura);
    Task<(int RowsAffected, string Mensaje)> AgregarExoneracionAsync(ExoneracionRequest exoneracion);
    Task<(int RowsAffected, string Mensaje)> AgregarFacturaAsync(AgregarFacturaRequest factura);
    Task<RegistrarFacturaResult> RegistrarFacturaCompletaAsync(RegistrarFacturaRequest request);

    // Persiste el resultado del envío a Hacienda (Clave/rutas/estado) en
    // facturacionfacturahst. SP_AgregarHistoricoFactura desactiva cualquier
    // historico previo (Estado=0) e inserta uno nuevo -- así que también sirve
    // para reintentos/reenvíos, no solo el primer envío.
    Task<(int RowsAffected, string Mensaje)> GuardarHistoricoFacturaAsync(
        int idFactura, string clave, string pathXml, string pathPdf, string pathRespuesta, int estadoEnvio, int estadoHacienda);
    Task<IEnumerable<ExoneracionResponse>> ObtenerExoneracionPorIdClienteAsync(int idCliente);
    Task<IEnumerable<ExoneracionCabysItem>> ObtenerExoneracionCabysPorIdAsync(int idExoneracion);
    Task<IEnumerable<FacturaPorConsecutivoResponse>> ObtenerFacturaPorConsecutivoYNegocioAsync(int idEntidad, string consecutivo);
    Task<IEnumerable<FacturaReferenciaResponse>> BuscarFacturaParaNotaAsync(int idEntidad, string consecutivo);
    Task<FacturaResponse?> ObtenerFacturaPorIdAsync(int idFactura);
    Task<IEnumerable<DetalleFacturaResponse>> ObtenerDetalleFacturaPorIdAsync(int idFactura);
    Task<IEnumerable<DocumentosPorNegocioResponse>> ObtenerFacturasPorNegocioAsync(int idEntidad);
    Task<HistoricoFacturaDto?> ObtenerHistoricoFacturaPorIdAsync(int idFactura);
}
