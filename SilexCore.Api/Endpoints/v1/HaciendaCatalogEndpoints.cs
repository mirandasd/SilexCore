using SilexCore.Api.Interfaces;
using SilexCore.Domain.Interfaces.Infraestructura.Services;

namespace SilexCore.Api.Endpoints.v1;

public class HaciendaCatalogEndpoints : IEndpointModule
{
    public void RegistrarEndpoints(IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup("/api/hacienda")
                       .WithTags("Hacienda Catalogos");

        // ==========================================
        // 1. Consultas Específicas / Búsquedas
        // ==========================================
        grupo.MapGet("/contribuyente/{identificacion}", async (string identificacion, IHaciendaCatalogServices service) =>
        {
            var result = await service.ObtenerContribuyenteAsync(identificacion);
            return result is not null ? Results.Ok(result) : Results.NotFound();
        })
        .WithName("ObtenerContribuyente");

        grupo.MapGet("/cabys", async (string termino, IHaciendaCatalogServices service) =>
            Results.Ok(await service.GetDetalleCabysAsync(termino)))
        .WithName("GetDetalleCabys");

        grupo.MapGet("/exoneracion/{autorizacion}", async (string autorizacion, IHaciendaCatalogServices service) =>
        {
            var result = await service.ObtenerExoneracionAsync(autorizacion);
            return result is not null ? Results.Ok(result) : Results.NotFound();
        })
        .WithName("ObtenerExoneracionHacienda");

        // ==========================================
        // 2. Ubicación Geográfica
        // ==========================================
        grupo.MapGet("/provincias", async (IHaciendaCatalogServices service) =>
            Results.Ok(await service.ObtenerProvinciasAsync()))
        .WithName("ObtenerProvincias");

        grupo.MapGet("/cantones/{idProvincia:int}", async (int idProvincia, IHaciendaCatalogServices service) =>
            Results.Ok(await service.ObtenerCantonesPorProvinciaAsync(idProvincia)))
        .WithName("ObtenerCantonesPorProvincia");

        grupo.MapGet("/distritos/{idProvincia:int}/{idCanton:int}", async (int idProvincia, int idCanton, IHaciendaCatalogServices service) =>
            Results.Ok(await service.ObtenerDistritosPorProvinciaYCantonAsync(idProvincia, idCanton)))
        .WithName("ObtenerDistritosPorProvinciaYCanton");

        // ==========================================
        // 3. Catálogos de Facturación
        // ==========================================
        grupo.MapGet("/tipos-comprobante", async (IHaciendaCatalogServices service) =>
            Results.Ok(await service.ObtenerTiposComprobanteAsync()))
        .WithName("ObtenerTiposComprobante");

        grupo.MapGet("/medios-pago", async (IHaciendaCatalogServices service) =>
            Results.Ok(await service.ObtenerMediosPagoAsync()))
        .WithName("ObtenerMediosPago");

        grupo.MapGet("/unidades-medida", async (IHaciendaCatalogServices service) =>
            Results.Ok(await service.ObtenerUnidadesMedidaAsync()))
        .WithName("ObtenerUnidadesMedida");

        grupo.MapGet("/tipos-tarifa", async (IHaciendaCatalogServices service) =>
            Results.Ok(await service.ObtenerTiposTarifaAsync()))
        .WithName("ObtenerTiposTarifa");

        grupo.MapGet("/tipos-documento-exoneracion", async (IHaciendaCatalogServices service) =>
            Results.Ok(await service.ObtenerTiposDocumentoExoneracionAsync()))
        .WithName("ObtenerTiposDocumentoExoneracion");

        grupo.MapGet("/tipos-proceso", async (IHaciendaCatalogServices service) =>
            Results.Ok(await service.ObtenerTiposProcesoAsync()))
        .WithName("ObtenerTiposProceso");

        grupo.MapGet("/tipos-impuesto", async (IHaciendaCatalogServices service) =>
            Results.Ok(await service.ObtenerTiposImpuestoAsync()))
        .WithName("ObtenerTiposImpuesto");

        grupo.MapGet("/porcentajes-exoneracion", async (IHaciendaCatalogServices service) =>
            Results.Ok(await service.ObtenerPorcentajesExoneracionAsync()))
        .WithName("ObtenerPorcentajesExoneracion");

        grupo.MapGet("/instituciones", async (IHaciendaCatalogServices service) =>
            Results.Ok(await service.ObtenerInstitucionesAsync()))
        .WithName("ObtenerInstituciones");

        grupo.MapGet("/tipos-identificacion", async (IHaciendaCatalogServices service) =>
            Results.Ok(await service.ObtenerTiposIdentificacionAsync()))
        .WithName("ObtenerTiposIdentificacion");

        grupo.MapGet("/estados-recepcion", async (IHaciendaCatalogServices service) =>
            Results.Ok(await service.ObtenerEstadosRecepcionAsync()))
        .WithName("ObtenerEstadosRecepcion");

        grupo.MapGet("/condiciones-impuesto", async (IHaciendaCatalogServices service) =>
            Results.Ok(await service.ObtenerCondicionesImpuestoAsync()))
        .WithName("ObtenerCondicionesImpuesto");

        grupo.MapGet("/condiciones-venta", async (IHaciendaCatalogServices service) =>
            Results.Ok(await service.ObtenerCondicionesVentaAsync()))
        .WithName("ObtenerCondicionesVenta");

        grupo.MapGet("/codigos-producto", async (IHaciendaCatalogServices service) =>
            Results.Ok(await service.ObtenerCodigosProductoAsync()))
        .WithName("ObtenerCodigosProducto");

        grupo.MapGet("/acciones-nota", async (IHaciendaCatalogServices service) =>
            Results.Ok(await service.ObtenerAccionesNotaAsync()))
        .WithName("ObtenerAccionesNota");

        grupo.MapGet("/monedas", async (IHaciendaCatalogServices service) =>
            Results.Ok(await service.GetMonedaAsync()))
        .WithName("GetMoneda");
    }
}
