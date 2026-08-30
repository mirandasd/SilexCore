namespace SilexCore.Domain.Interfaces.Infraestructura.Services;

public interface IBitacoraService
{
    Task RegistrarNegocioBitacoraAsync(string mensaje, string payload);
    Task RegistrarErrorBitacoraAsync(string mensaje, string payload);
}
