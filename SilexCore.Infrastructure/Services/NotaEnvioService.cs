using Karin.InvoicingService.Client.Dtos;
using Karin.InvoicingService.Client.Enums;
using Karin.InvoicingService.Client.Interface;
using SilexCore.Domain.Dtos;
using SilexCore.Domain.Interfaces.Infraestructura.Persistence;
using SilexCore.Domain.Interfaces.Infraestructura.Services;

namespace SilexCore.Infrastructure.Services;

public class NotaEnvioService(IInvoicingServiceClient invoicingClient, INotaRepository notaRepo) : INotaEnvioService
{
    public async Task<EnviarFacturaResult> EnviarNotaAsync(int idNota)
    {
        // NuevoConsecutivo=true: primer envío -- genera y guarda la Clave, igual que
        // FacturaEnvioService. Si en el futuro se agrega un "reenviar", eso debe usar
        // false para reusar la Clave ya generada en vez de crear una distinta.
        var response = await invoicingClient.EnviarComprobanteAsync(TipoComprobante.NotaCredito, new ComprobanteRequest
        {
            IdDocumento = idNota,
            NuevoConsecutivo = true
        });

        if (!response.IsSuccessStatusCode || response.Content is null)
        {
            var detalle = response.Error?.Message ?? response.ReasonPhrase ?? "Error desconocido";
            throw new InvalidOperationException($"InvoicingService no pudo procesar la nota: {detalle}");
        }

        var comprobante = response.Content;
        var estadoHacienda = MapearEstadoHacienda(comprobante.Estado);
        var estadoEnvio = string.Equals(comprobante.Estado, "ErrorEnvio", StringComparison.OrdinalIgnoreCase) ? 0 : 1;

        var (rows, mensajeHistorico) = await notaRepo.GuardarHistoricoNotaAsync(
            idNota,
            comprobante.Clave,
            comprobante.RutaArchivo,
            pathPdf: string.Empty,
            comprobante.RutaRespuesta,
            estadoEnvio,
            estadoHacienda);

        if (rows <= 0)
            throw new InvalidOperationException($"La nota se envió a Hacienda pero no se pudo guardar el histórico: {mensajeHistorico}");

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
