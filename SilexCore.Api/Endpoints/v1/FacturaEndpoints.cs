using SilexCore.Api.Interfaces;
using SilexCore.Domain.Interfaces.Infraestructura.Persistence;

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
        }
    }
}
