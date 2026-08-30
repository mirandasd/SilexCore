namespace SilexCore.Domain.Interfaces.Infraestructura.Services;

public interface IReporteService
{
    Task<(byte[] Contenido, string ContentType)> GenerarReporteAsync(string tipoReporte, object datos, string formato = "pdf");
}
