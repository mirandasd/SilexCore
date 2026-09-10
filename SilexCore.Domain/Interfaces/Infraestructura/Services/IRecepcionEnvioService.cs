using SilexCore.Domain.Dtos;

namespace SilexCore.Domain.Interfaces.Infraestructura.Services;

// Envía un mensaje receptor (aceptación/rechazo de un documento recibido) a
// Hacienda vía InvoicingService (firma + envío) y persiste el resultado
// (Consecutivo/Estado/rutas) en facturacionrecepcionhst.
public interface IRecepcionEnvioService
{
    Task<EnviarFacturaResult> EnviarRecepcionAsync(int idRecepcionDocumento);
}
