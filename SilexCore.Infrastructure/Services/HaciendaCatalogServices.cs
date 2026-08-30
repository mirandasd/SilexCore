using Karin.HaciendaCatalogService.Client.Interface;
using Mapster;
using Refit;
using SilexCore.Domain.Dtos;
using SilexCore.Domain.Interfaces.Infraestructura.Services;

namespace SilexCore.Infrastructure.Services;

public class HaciendaCatalogServices(IHaciendaCatalogServiceClient catalogServiceClient) : IHaciendaCatalogServices
{
    public async Task<ContribuyenteResponse?> ObtenerContribuyenteAsync(string identificacion)
    {
        var result = await catalogServiceClient.ObtenerContribuyenteAsync(identificacion);
        return result?.Adapt<ContribuyenteResponse>();
    }

    public async Task<ExoneracionHaciendaResponse?> ObtenerExoneracionAsync(string autorizacion)
    {
        try
        {
            var result = await catalogServiceClient.GetExoneracionAsync(autorizacion);
            return result?.Adapt<ExoneracionHaciendaResponse>();
        }
        catch (ApiException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task<IEnumerable<ProvinciaResponse>> ObtenerProvinciasAsync()
    {
        var result = await catalogServiceClient.ObtenerProvinciasAsync();
        return result.Adapt<IEnumerable<ProvinciaResponse>>();
    }

    public async Task<IEnumerable<CantonResponse>> ObtenerCantonesPorProvinciaAsync(int idProvincia)
    {
        var result = await catalogServiceClient.ObtenerCantonesPorProvinciaAsync(idProvincia);
        return result.Adapt<IEnumerable<CantonResponse>>();
    }

    public async Task<IEnumerable<DistritoResponse>> ObtenerDistritosPorProvinciaYCantonAsync(int idProvincia, int idCanton)
    {
        var result = await catalogServiceClient.ObtenerDistritosPorProvinciaYCantonAsync(idProvincia, idCanton);
        return result.Adapt<IEnumerable<DistritoResponse>>();
    }

    public async Task<IEnumerable<OpcionDeFacturacionResponse>> ObtenerTiposComprobanteAsync()
    {
        var result = await catalogServiceClient.ObtenerTiposComprobanteAsync();
        return result.Adapt<IEnumerable<OpcionDeFacturacionResponse>>();
    }

    public async Task<IEnumerable<OpcionDeFacturacionResponse>> ObtenerMediosPagoAsync()
    {
        var result = await catalogServiceClient.ObtenerMediosPagoAsync();
        return result.Adapt<IEnumerable<OpcionDeFacturacionResponse>>();
    }

    public async Task<IEnumerable<OpcionDeFacturacionResponse>> ObtenerUnidadesMedidaAsync()
    {
        var result = await catalogServiceClient.ObtenerUnidadesMedidaAsync();
        return result.Adapt<IEnumerable<OpcionDeFacturacionResponse>>();
    }

    public async Task<IEnumerable<OpcionDeFacturacionConValorResponse>> ObtenerTiposTarifaAsync()
    {
        var result = await catalogServiceClient.ObtenerTiposTarifaAsync();
        return result.Adapt<IEnumerable<OpcionDeFacturacionConValorResponse>>();
    }

    public async Task<IEnumerable<OpcionDeFacturacionResponse>> ObtenerTiposDocumentoExoneracionAsync()
    {
        var result = await catalogServiceClient.ObtenerTiposDocumentoExoneracionAsync();
        return result.Adapt<IEnumerable<OpcionDeFacturacionResponse>>();
    }

    public async Task<IEnumerable<OpcionDeFacturacionResponse>> ObtenerTiposProcesoAsync()
    {
        var result = await catalogServiceClient.ObtenerTiposProcesoAsync();
        return result.Adapt<IEnumerable<OpcionDeFacturacionResponse>>();
    }

    public async Task<IEnumerable<OpcionDeFacturacionResponse>> ObtenerTiposImpuestoAsync()
    {
        var result = await catalogServiceClient.ObtenerTiposImpuestoAsync();
        return result.Adapt<IEnumerable<OpcionDeFacturacionResponse>>();
    }

    public async Task<IEnumerable<OpcionDeFacturacionResponse>> ObtenerPorcentajesExoneracionAsync()
    {
        var result = await catalogServiceClient.ObtenerPorcentajesExoneracionAsync();
        return result.Adapt<IEnumerable<OpcionDeFacturacionResponse>>();
    }

    public async Task<IEnumerable<OpcionDeFacturacionResponse>> ObtenerInstitucionesAsync()
    {
        var result = await catalogServiceClient.ObtenerInstitucionesAsync();
        return result.Adapt<IEnumerable<OpcionDeFacturacionResponse>>();
    }

    public async Task<IEnumerable<OpcionDeFacturacionResponse>> ObtenerTiposIdentificacionAsync()
    {
        var result = await catalogServiceClient.ObtenerTiposIdentificacionAsync();
        return result.Adapt<IEnumerable<OpcionDeFacturacionResponse>>();
    }

    public async Task<IEnumerable<OpcionDeFacturacionResponse>> ObtenerEstadosRecepcionAsync()
    {
        var result = await catalogServiceClient.ObtenerEstadosRecepcionAsync();
        return result.Adapt<IEnumerable<OpcionDeFacturacionResponse>>();
    }

    public async Task<IEnumerable<OpcionDeFacturacionResponse>> ObtenerCondicionesImpuestoAsync()
    {
        var result = await catalogServiceClient.ObtenerCondicionesImpuestoAsync();
        return result.Adapt<IEnumerable<OpcionDeFacturacionResponse>>();
    }

    public async Task<IEnumerable<OpcionDeFacturacionResponse>> ObtenerCondicionesVentaAsync()
    {
        var result = await catalogServiceClient.ObtenerCondicionesVentaAsync();
        return result.Adapt<IEnumerable<OpcionDeFacturacionResponse>>();
    }

    public async Task<IEnumerable<OpcionDeFacturacionResponse>> ObtenerCodigosProductoAsync()
    {
        var result = await catalogServiceClient.ObtenerCodigosProductoAsync();
        return result.Adapt<IEnumerable<OpcionDeFacturacionResponse>>();
    }

    public async Task<IEnumerable<OpcionDeFacturacionResponse>> ObtenerAccionesNotaAsync()
    {
        var result = await catalogServiceClient.ObtenerAccionesNotaAsync();
        return result.Adapt<IEnumerable<OpcionDeFacturacionResponse>>();
    }

    public async Task<IEnumerable<OpcionDeFacturacionConValorResponse>> GetMonedaAsync()
    {
        var result = await catalogServiceClient.GetMonedaAsync();
        return result.Adapt<IEnumerable<OpcionDeFacturacionConValorResponse>>();
    }

    public async Task<List<CabysResponse>> GetDetalleCabysAsync(string termino)
    {
        var result = await catalogServiceClient.GetDetalleCabysAsync(termino);
        return result.Adapt<List<CabysResponse>>();
    }
}