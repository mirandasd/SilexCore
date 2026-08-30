namespace SilexCore.Domain.Interfaces.Infraestructura.Services;

public interface INotaReporteService
{
    Task<(byte[] Contenido, string ContentType)?> GenerarReporteAsync(int idNota, string formato = "pdf");
}
