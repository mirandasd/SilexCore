using System.Text.Json;
using SilexCore.Api.Interfaces;
using SilexCore.Domain.Dtos;
using SilexCore.Domain.Interfaces.Infraestructura.Persistence;
using SilexCore.Domain.Interfaces.Infraestructura.Services;

namespace SilexCore.Api.Endpoints.v1;

public class NotaEndpoints : IEndpointModule
{
    public void RegistrarEndpoints(IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup("/api/nota")
                       .WithTags("Notas")
                       .RequireAuthorization();

        // --- CONSULTAS (GET) ---

        // Obtener notas (crédito/débito) por negocio
        grupo.MapGet("/negocio/{idEntidad:int}", async (int idEntidad, INotaRepository notaRepo) =>
        {
            var notas = await notaRepo.ObtenerNotasPorNegocioAsync(idEntidad);
            return Results.Ok(notas);
        })
        .WithName("ObtenerNotasPorNegocio");

        // --- CREACIÓN (POST) ---

        // Registrar nota de crédito sobre una factura existente: toma el
        // consecutivo vigente, crea la nota y lo avanza -- todo en una transacción.
        grupo.MapPost("/", async (RegistrarNotaCreditoRequest nota, INotaRepository notaRepo, IBitacoraService bitacora) =>
        {
            var payload = JsonSerializer.Serialize(nota);
            try
            {
                var resultado = await notaRepo.RegistrarNotaCreditoAsync(nota);
                await bitacora.RegistrarNegocioBitacoraAsync(
                    $"Nota de crédito registrada: consecutivo {resultado.Consecutivo}, factura {nota.IdFactura}", payload);
                return Results.Created("/api/nota", resultado);
            }
            catch (Exception ex)
            {
                await bitacora.RegistrarErrorBitacoraAsync($"Error al registrar nota para la factura {nota.IdFactura}: {ex.Message}", payload);
                return Results.BadRequest(new { mensaje = ex.Message });
            }
        })
        .WithName("AgregarNota");

        // Representación gráfica (PDF/HTML) de la nota, vía ReportingService
        grupo.MapGet("/{idNota:int}/reporte", async (int idNota, string? formato, INotaReporteService reporteService) =>
        {
            var resultado = await reporteService.GenerarReporteAsync(idNota, formato ?? "pdf");
            if (resultado is null) return Results.NotFound();

            var (contenido, contentType) = resultado.Value;
            return Results.File(contenido, contentType, contentType == "application/pdf" ? $"nota-{idNota}.pdf" : null);
        });
    }
}
