using SilexCore.Domain.Dtos;

namespace SilexCore.Domain.Interfaces.Infraestructura.Services;

// Envía una factura ya registrada a Hacienda vía InvoicingService (firma + envío)
// y persiste el resultado (Clave/Estado/rutas) en facturacionfacturahst.
public interface IFacturaEnvioService
{
    Task<EnviarFacturaResult> EnviarFacturaAsync(int idFactura);
}
