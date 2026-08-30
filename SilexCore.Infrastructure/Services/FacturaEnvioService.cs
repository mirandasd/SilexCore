using Karin.InvoicingService.Client.Dtos;
using Karin.InvoicingService.Client.Enums;
using Karin.InvoicingService.Client.Interface;
using SilexCore.Domain.Dtos;
using SilexCore.Domain.Interfaces.Infraestructura.Persistence;
using SilexCore.Domain.Interfaces.Infraestructura.Services;

namespace SilexCore.Infrastructure.Services;

public class FacturaEnvioService(IInvoicingServiceClient invoicingClient, IFacturaRepository facturaRepo) : IFacturaEnvioService
{
    public async Task<EnviarFacturaResult> EnviarFacturaAsync(int idFactura)
    {
        var response = await invoicingClient.EnviarComprobanteAsync(TipoComprobante.Factura, new ComprobanteRequest
        {
            IdDocumento = idFactura,
            NuevoConsecutivo = true
        });

        if (!response.IsSuccessStatusCode || response.Content is null)
        {
            var detalle = response.Error?.Message ?? response.ReasonPhrase ?? "Error desconocido";
            throw new InvalidOperationException($"InvoicingService no pudo procesar la factura: {detalle}");
        }

        var comprobante = response.Content;
        var estadoHacienda = MapearEstadoHacienda(comprobante.Estado);
        var estadoEnvio = string.Equals(comprobante.Estado, "ErrorEnvio", StringComparison.OrdinalIgnoreCase) ? 0 : 1;

        var (rows, mensajeHistorico) = await facturaRepo.GuardarHistoricoFacturaAsync(
            idFactura,
            comprobante.Clave,
            comprobante.RutaArchivo,
            pathPdf: string.Empty,
            comprobante.RutaRespuesta,
            estadoEnvio,
            estadoHacienda);

        if (rows <= 0)
            throw new InvalidOperationException($"La factura se envió a Hacienda pero no se pudo guardar el histórico: {mensajeHistorico}");

        return new EnviarFacturaResult
        {
            Clave = comprobante.Clave,
            Consecutivo = comprobante.Consecutivo,
            Estado = comprobante.Estado,
            Mensaje = comprobante.Mensaje
        };
    }

    // EstadoHacienda en facturacionfacturahst: 1 = Aceptada, 3 = Rechazada, cualquier
    // otro valor se muestra como "Pendiente" en el resto del sistema (ObtenerFacturasPorNegocio,
    // etc.) -- no se inventa un tercer estado nuevo, se respeta esa convención ya existente.
    private static int MapearEstadoHacienda(string estado) => estado?.Trim().ToLowerInvariant() switch
    {
        "aceptado" => 1,
        "rechazado" => 3,
        _ => 2
    };
}
