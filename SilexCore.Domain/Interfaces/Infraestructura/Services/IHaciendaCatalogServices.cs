using SilexCore.Domain.Dtos;

namespace SilexCore.Domain.Interfaces.Infraestructura.Services;

public interface IHaciendaCatalogServices
{
    Task<List<CabysResponse>> GetDetalleCabysAsync(string termino);
    Task<IEnumerable<OpcionDeFacturacionConValorResponse>> GetMonedaAsync();
    Task<IEnumerable<OpcionDeFacturacionResponse>> ObtenerAccionesNotaAsync();
    Task<IEnumerable<CantonResponse>> ObtenerCantonesPorProvinciaAsync(int idProvincia);
    Task<IEnumerable<OpcionDeFacturacionResponse>> ObtenerCodigosProductoAsync();
    Task<IEnumerable<OpcionDeFacturacionResponse>> ObtenerCondicionesImpuestoAsync();
    Task<ContribuyenteResponse?> ObtenerContribuyenteAsync(string identificacion);
    Task<IEnumerable<DistritoResponse>> ObtenerDistritosPorProvinciaYCantonAsync(int idProvincia, int idCanton);
    Task<IEnumerable<OpcionDeFacturacionResponse>> ObtenerEstadosRecepcionAsync();
    Task<IEnumerable<OpcionDeFacturacionResponse>> ObtenerInstitucionesAsync();
    Task<IEnumerable<OpcionDeFacturacionResponse>> ObtenerMediosPagoAsync();
    Task<IEnumerable<OpcionDeFacturacionResponse>> ObtenerPorcentajesExoneracionAsync();
    Task<IEnumerable<ProvinciaResponse>> ObtenerProvinciasAsync();
    Task<IEnumerable<OpcionDeFacturacionResponse>> ObtenerTiposComprobanteAsync();
    Task<IEnumerable<OpcionDeFacturacionResponse>> ObtenerTiposDocumentoExoneracionAsync();
    Task<IEnumerable<OpcionDeFacturacionResponse>> ObtenerTiposIdentificacionAsync();
    Task<IEnumerable<OpcionDeFacturacionResponse>> ObtenerTiposImpuestoAsync();
    Task<IEnumerable<OpcionDeFacturacionResponse>> ObtenerTiposProcesoAsync();
    Task<IEnumerable<OpcionDeFacturacionResponse>> ObtenerTiposTarifaAsync();
    Task<IEnumerable<OpcionDeFacturacionResponse>> ObtenerUnidadesMedidaAsync();
}
