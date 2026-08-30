using System.Text.Json;
using SilexCore.Api.Interfaces;
using SilexCore.Domain.Dtos;
using SilexCore.Domain.Interfaces.Infraestructura.Persistence;
using SilexCore.Domain.Interfaces.Infraestructura.Services;

namespace SilexCore.Api.Endpoints.v1;

// "Apilado" en silex-app: registro diario de producción de material (piedra cuarta,
// polvo de piedra, arena, lastre) por negocio.
public class ProduccionMaterialEndpoints : IEndpointModule
{
    public void RegistrarEndpoints(IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup("/api/produccion-material")
                       .WithTags("Apilado")
                       .RequireAuthorization();

        grupo.MapGet("/negocio/{idEntidad:int}", async (int idEntidad, IProduccionMaterialRepository repo) =>
        {
            var registros = await repo.ObtenerPorNegocioAsync(idEntidad);
            return Results.Ok(registros);
        })
        .WithName("ObtenerProduccionMaterialPorNegocio");

        // Balance de apilado = producido - vendido (facturado) por material, en un rango de fechas.
        // Pensado para alimentar el futuro reporte de apilado, no solo la tabla de registros diarios.
        grupo.MapGet("/balance/negocio/{idEntidad:int}", async (int idEntidad, DateTime fechaInicio, DateTime fechaFin, IProduccionMaterialRepository repo) =>
        {
            var balance = await repo.ObtenerBalanceAsync(idEntidad, fechaInicio, fechaFin);
            return Results.Ok(balance);
        })
        .WithName("ObtenerBalanceApiladoPorNegocio");

        // Mismo balance de arriba, pero ya renderizado como reporte (ReportingService.Api).
        grupo.MapGet("/balance/negocio/{idEntidad:int}/reporte", async (int idEntidad, DateTime fechaInicio, DateTime fechaFin, string? formato, IProduccionMaterialRepository repo, IUsuarioRepository usuarioRepo, IReporteService reportes) =>
        {
            var balance = await repo.ObtenerBalanceAsync(idEntidad, fechaInicio, fechaFin);
            var negocios = await usuarioRepo.ListarNegociosAsync();
            var negocio = negocios.FirstOrDefault(n => n.IdEntidad == idEntidad)?.RazonSocial ?? $"Negocio #{idEntidad}";

            var datos = new
            {
                negocio,
                fecha_inicio = fechaInicio.ToString("yyyy-MM-dd"),
                fecha_fin = fechaFin.ToString("yyyy-MM-dd"),
                generado_en = DateTime.Now.ToString("yyyy-MM-dd HH:mm"),
                materiales = balance.Select(b => new
                {
                    material = b.Material,
                    total_producido = b.TotalProducido,
                    total_vendido = b.TotalVendido,
                    balance = b.Balance
                })
            };

            var formatoSolicitado = formato ?? "pdf";
            var (contenido, contentType) = await reportes.GenerarReporteAsync("ApiladoBalance", datos, formatoSolicitado);

            return Results.File(contenido, contentType, contentType == "application/pdf" ? "balance-apilado.pdf" : null);
        })
        .WithName("GenerarReporteBalanceApilado");

        grupo.MapPost("/", async (ProduccionMaterialRequest registro, IProduccionMaterialRepository repo, IBitacoraService bitacora) =>
        {
            var resultado = await repo.AgregarAsync(registro);
            var payload = JsonSerializer.Serialize(registro);

            if (resultado.IdProduccion <= 0)
            {
                await bitacora.RegistrarErrorBitacoraAsync($"Error al registrar producción de material: {resultado.Mensaje}", payload);
                return Results.BadRequest(new { mensaje = resultado.Mensaje });
            }

            await bitacora.RegistrarNegocioBitacoraAsync($"Producción de material registrada para negocio {registro.IdEntidad}", payload);
            return Results.Created($"/api/produccion-material/{resultado.IdProduccion}", resultado);
        })
        .WithName("AgregarProduccionMaterial");

        grupo.MapPut("/", async (ActualizarProduccionMaterialRequest registro, IProduccionMaterialRepository repo, IBitacoraService bitacora) =>
        {
            var (rows, mensaje) = await repo.ActualizarAsync(registro);
            var payload = JsonSerializer.Serialize(registro);

            if (rows <= 0)
            {
                await bitacora.RegistrarErrorBitacoraAsync($"Error al actualizar producción {registro.IdProduccion}: {mensaje}", payload);
                return Results.BadRequest(new { mensaje });
            }

            await bitacora.RegistrarNegocioBitacoraAsync($"Producción de material actualizada: {registro.IdProduccion}", payload);
            return Results.Ok(new { mensaje });
        })
        .WithName("ActualizarProduccionMaterial");
    }
}
