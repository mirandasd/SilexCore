namespace SilexCore.Domain.Interfaces.Infraestructura.Services;

public interface IFacturaReporteService
{
    Task<(byte[] Contenido, string ContentType)?> GenerarReporteAsync(int idFactura, string formato = "pdf");
}
