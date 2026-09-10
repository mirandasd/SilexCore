using Karin.InvoicingService.Client.Dtos;
using Karin.InvoicingService.Client.Enums;
using Karin.InvoicingService.Client.Interface;
using SilexCore.Domain.Dtos;
using SilexCore.Domain.Interfaces.Infraestructura.Persistence;
using SilexCore.Domain.Interfaces.Infraestructura.Services;

namespace SilexCore.Infrastructure.Services;

public class RecepcionEnvioService(IInvoicingServiceClient invoicingClient, IRecepcionRepository recepcionRepo) : IRecepcionEnvioService
{
    public async Task<EnviarFacturaResult> EnviarRecepcionAsync(int idRecepcionDocumento)
    {
        var response = await invoicingClient.EnviarComprobanteAsync(TipoComprobante.MensajeReceptor, new ComprobanteRequest
        {
            IdDocumento = idRecepcionDocumento,
            NuevoConsecutivo = true
        });

        if (!response.IsSuccessStatusCode || response.Content is null)
        {
            var detalle = response.Error?.Message ?? response.ReasonPhrase ?? "Error desconocido";
            throw new InvalidOperationException($"InvoicingService no pudo procesar la recepción: {detalle}");
        }

        var comprobante = response.Content;
        var estadoHacienda = MapearEstadoHacienda(comprobante.Estado);
        var estadoEnvio = string.Equals(comprobante.Estado, "ErrorEnvio", StringComparison.OrdinalIgnoreCase) ? 0 : 1;

        // A diferencia de factura/nota, el historial de recepción guarda el
        // Consecutivo propio del mensaje receptor -- la Clave que trae el
        // comprobante es la del documento del emisor que se está confirmando,
        // no una clave nueva de este mensaje.
        var (rows, mensajeHistorico) = await recepcionRepo.GuardarHistoricoRecepcionAsync(
            idRecepcionDocumento,
            comprobante.Consecutivo,
            comprobante.RutaArchivo,
            pathPdf: string.Empty,
            comprobante.RutaRespuesta,
            estadoEnvio,
            estadoHacienda);

        if (rows <= 0)
            throw new InvalidOperationException($"La recepción se envió a Hacienda pero no se pudo guardar el histórico: {mensajeHistorico}");

        return new EnviarFacturaResult
        {
            Clave = comprobante.Clave,
            Consecutivo = comprobante.Consecutivo,
            Estado = comprobante.Estado,
            Mensaje = comprobante.Mensaje
        };
    }

    // Misma convención que FacturaEnvioService: 1 = Aceptada, 3 = Rechazada, cualquier
    // otro valor se muestra como "Pendiente".
    private static int MapearEstadoHacienda(string estado) => estado?.Trim().ToLowerInvariant() switch
    {
        "aceptado" => 1,
        "rechazado" => 3,
        _ => 2
    };
}
