using System.Text.Json;
using SilexCore.Api.Interfaces;
using SilexCore.Domain.Dtos;
using SilexCore.Domain.Interfaces.Infraestructura.Persistence;
using SilexCore.Domain.Interfaces.Infraestructura.Services;

namespace SilexCore.Api.Endpoints.v1
{
    public class FacturaEndpoints : IEndpointModule
    {
        public void RegistrarEndpoints(IEndpointRouteBuilder app)
        {
            var grupo = app.MapGroup("/api/factura");

            // --- CONSULTAS (GET) ---

            // Obtener facturas por negocio
            grupo.MapGet("/negocio/{idEntidad:int}", async (int idEntidad, IFacturaRepository facturaRepo) =>
            {
                var facturas = await facturaRepo.ObtenerFacturasPorNegocioAsync(idEntidad);
                return Results.Ok(facturas);
            });

            // Obtener factura por consecutivo y negocio
            grupo.MapGet("/negocio/{idEntidad:int}/consecutivo/{consecutivo}", async (int idEntidad, string consecutivo, IFacturaRepository facturaRepo) =>
            {
                var facturas = await facturaRepo.ObtenerFacturaPorConsecutivoYNegocioAsync(idEntidad, consecutivo);
                return Results.Ok(facturas);
            });

            // Buscar factura(s) por sufijo de consecutivo -- para elegir el documento
            // de referencia al armar una nota de crédito.
            grupo.MapGet("/negocio/{idEntidad:int}/buscar-para-nota/{consecutivo}", async (int idEntidad, string consecutivo, IFacturaRepository facturaRepo) =>
            {
                var facturas = await facturaRepo.BuscarFacturaParaNotaAsync(idEntidad, consecutivo);
                return Results.Ok(facturas);
            })
            .WithName("BuscarFacturaParaNota");

            // Obtener factura completa por ID
            grupo.MapGet("/{idFactura:int}", async (int idFactura, IFacturaRepository facturaRepo) =>
            {
                var factura = await facturaRepo.ObtenerFacturaPorIdAsync(idFactura);
                return factura is not null ? Results.Ok(factura) : Results.NotFound();
            });

            // Obtener histórico de factura por ID
            grupo.MapGet("/{idFactura:int}/historico", async (int idFactura, IFacturaRepository facturaRepo) =>
            {
                var historico = await facturaRepo.ObtenerHistoricoFacturaPorIdAsync(idFactura);
                return historico is not null ? Results.Ok(historico) : Results.NotFound();
            });

            grupo.MapGet("/cliente/{idCliente:int}/exoneraciones", async (int idCliente, IFacturaRepository facturaRepo) =>
            {
                var exoneraciones = await facturaRepo.ObtenerExoneracionPorIdClienteAsync(idCliente);
                return exoneraciones != null && exoneraciones.Any()
                    ? Results.Ok(exoneraciones)
                    : Results.NotFound($"No se encontraron exoneraciones activas para el cliente con ID {idCliente}.");
            });

            // CABYS autorizados de una exoneración ya guardada (solo aplica si PoseeCabys)
            grupo.MapGet("/exoneracion/{idExoneracion:int}/cabys", async (int idExoneracion, IFacturaRepository facturaRepo) =>
            {
                var cabys = await facturaRepo.ObtenerExoneracionCabysPorIdAsync(idExoneracion);
                return Results.Ok(cabys);
            });

            // --- CREACIÓN (POST) ---

            // Registra una exoneración para un cliente (o la recarga, si ya existía: la
            // SP hace get-or-create por numeroDocumento+autorizacion+tipoAutorizacion).
            grupo.MapPost("/exoneracion", async (ExoneracionRequest exoneracion, IFacturaRepository facturaRepo) =>
            {
                var (rows, mensaje) = await facturaRepo.AgregarExoneracionAsync(exoneracion);
                return rows > 0 ? Results.Ok(new { idExoneracion = mensaje }) : Results.BadRequest(mensaje);
            });

            // Registra la factura completa: cliente (se crea si hace falta), consecutivo,
            // encabezado, detalle (con o sin exoneración por línea) y actualiza el
            // consecutivo al final. Todo en una sola transacción.
            grupo.MapPost("/", async (RegistrarFacturaRequest factura, IFacturaRepository facturaRepo, IBitacoraService bitacora) =>
            {
                var payload = JsonSerializer.Serialize(factura);
                try
                {
                    var resultado = await facturaRepo.RegistrarFacturaCompletaAsync(factura);
                    await bitacora.RegistrarNegocioBitacoraAsync(
                        $"Factura registrada: consecutivo {resultado.Consecutivo}, cliente {resultado.IdCliente}", payload);
                    return Results.Created($"/api/factura/{resultado.IdFactura}", resultado);
                }
                catch (Exception ex)
                {
                    await bitacora.RegistrarErrorBitacoraAsync($"Error al registrar factura: {ex.Message}", payload);
                    return Results.BadRequest(new { mensaje = ex.Message });
                }
            })
            .WithName("RegistrarFactura");

            // Envía una factura ya registrada a Hacienda (firma + envío vía
            // InvoicingService) y guarda Clave/Estado/rutas en facturacionfacturahst.
            grupo.MapPost("/{idFactura:int}/enviar", async (int idFactura, IFacturaEnvioService facturaEnvio, IBitacoraService bitacora) =>
            {
                try
                {
                    var resultado = await facturaEnvio.EnviarFacturaAsync(idFactura);
                    await bitacora.RegistrarNegocioBitacoraAsync(
                        $"Factura {idFactura} enviada a Hacienda: clave {resultado.Clave}, estado {resultado.Estado}", string.Empty);
                    return Results.Ok(resultado);
                }
                catch (Exception ex)
                {
                    await bitacora.RegistrarErrorBitacoraAsync($"Error al enviar la factura {idFactura} a Hacienda: {ex.Message}", string.Empty);
                    return Results.BadRequest(new { mensaje = ex.Message });
                }
            })
            .WithName("EnviarFactura");

            // Representación gráfica (PDF/HTML) de la factura, vía ReportingService
            grupo.MapGet("/{idFactura:int}/reporte", async (int idFactura, string? formato, IFacturaReporteService reporteService) =>
            {
                var resultado = await reporteService.GenerarReporteAsync(idFactura, formato ?? "pdf");
                if (resultado is null) return Results.NotFound();

                var (contenido, contentType) = resultado.Value;
                return Results.File(contenido, contentType, contentType == "application/pdf" ? $"factura-{idFactura}.pdf" : null);
            });
        }
    }
}
