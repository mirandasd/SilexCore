using System.Text.Json;
using SilexCore.Api.Interfaces;
using SilexCore.Domain.Dtos;
using SilexCore.Domain.Interfaces.Infraestructura.Persistence;
using SilexCore.Domain.Interfaces.Infraestructura.Services;

namespace SilexCore.Api.Endpoints.v1;

public class RecepcionEndpoints : IEndpointModule
{
    public void RegistrarEndpoints(IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup("/api/recepcion")
                       .WithTags("Recepcion")
                       .RequireAuthorization();

        // --- CONSULTAS (GET) ---

        // Obtener documentos de recepción por negocio
        grupo.MapGet("/negocio/{idEntidad:int}", async (int idEntidad, IRecepcionRepository recepcionRepo) =>
        {
            var recepciones = await recepcionRepo.ObtenerRecepcionesPorNegocioAsync(idEntidad);
            return Results.Ok(recepciones);
        })
        .WithName("ObtenerRecepcionesPorNegocio");

        // Validar si una clave de Hacienda ya fue recibida (para evitar duplicados)
        grupo.MapGet("/validar-clave/{clave}", async (string clave, IRecepcionRepository recepcionRepo) =>
        {
            var idExistente = await recepcionRepo.ValidarClaveRecepcionDeDocumentoAsync(clave);
            return Results.Ok(new { Existe = idExistente is not null, IdRecepcion = idExistente });
        })
        .WithName("ValidarClaveRecepcionDeDocumento");

        // Representación gráfica (PDF/HTML) de la recepción, vía ReportingService
        grupo.MapGet("/{idRecepcionDocumento:int}/reporte", async (int idRecepcionDocumento, string? formato, IRecepcionReporteService reporteService) =>
        {
            var resultado = await reporteService.GenerarReporteAsync(idRecepcionDocumento, formato ?? "pdf");
            if (resultado is null) return Results.NotFound();

            var (contenido, contentType) = resultado.Value;
            return Results.File(contenido, contentType, contentType == "application/pdf" ? $"recepcion-{idRecepcionDocumento}.pdf" : null);
        })
        .WithName("GenerarReporteRecepcion");

        // --- CREACIÓN (POST) ---

        // Registrar un documento de recepción (factura de un tercero)
        grupo.MapPost("/", async (RecepcionRequest recepcion, IRecepcionRepository recepcionRepo, IBitacoraService bitacora, IMensajeriaService mensajeria, IConfiguration configuration) =>
        {
            var (rows, mensaje) = await recepcionRepo.AgregarRecepcionAsync(recepcion);
            var payload = JsonSerializer.Serialize(recepcion);

            if (rows <= 0)
            {
                await bitacora.RegistrarErrorBitacoraAsync($"Error al registrar recepción de {recepcion.NombreEmisor}: {mensaje}", payload);
                return Results.BadRequest(mensaje);
            }

            await bitacora.RegistrarNegocioBitacoraAsync($"Recepción registrada de {recepcion.NombreEmisor} por {recepcion.Total:C}", payload);

            var correoNotificacion = configuration["NotificacionesSettings:CorreoCuentasPorPagar"];
            if (!string.IsNullOrWhiteSpace(correoNotificacion))
            {
                await mensajeria.EnviarNotificacionAsync(
                    correoNotificacion,
                    "Nuevo documento de recepción registrado",
                    $"Se registró un nuevo documento de recepción del emisor {recepcion.NombreEmisor} por un total de {recepcion.Total:C}.");
            }

            return Results.Created("/api/recepcion", new { mensaje });
        })
        .WithName("AgregarRecepcion");

        // Envía el mensaje receptor (aceptación/rechazo) a Hacienda (firma + envío vía
        // InvoicingService) y guarda Consecutivo/Estado/rutas en facturacionrecepcionhst.
        grupo.MapPost("/{idRecepcionDocumento:int}/enviar", async (int idRecepcionDocumento, IRecepcionEnvioService recepcionEnvio, IBitacoraService bitacora) =>
        {
            try
            {
                var resultado = await recepcionEnvio.EnviarRecepcionAsync(idRecepcionDocumento);
                await bitacora.RegistrarNegocioBitacoraAsync(
                    $"Recepción enviada a Hacienda: consecutivo {resultado.Consecutivo}, estado {resultado.Estado}", string.Empty);
                return Results.Ok(resultado);
            }
            catch (Exception ex)
            {
                await bitacora.RegistrarErrorBitacoraAsync($"Error al enviar la recepción {idRecepcionDocumento} a Hacienda: {ex.Message}", string.Empty);
                return Results.BadRequest(new { mensaje = ex.Message });
            }
        })
        .WithName("EnviarRecepcion");
    }
}
