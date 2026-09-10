using SilexCore.Domain.Dtos;

namespace SilexCore.Domain.Interfaces.Infraestructura.Services;

// Envía una nota de crédito ya registrada a Hacienda vía InvoicingService (firma + envío)
// y persiste el resultado (Clave/Estado/rutas) en facturacionnotahst.
public interface INotaEnvioService
{
    Task<EnviarFacturaResult> EnviarNotaAsync(int idNota);
}
