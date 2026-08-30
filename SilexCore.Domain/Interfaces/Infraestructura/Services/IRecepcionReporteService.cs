namespace SilexCore.Domain.Interfaces.Infraestructura.Services;

public interface IRecepcionReporteService
{
    Task<(byte[] Contenido, string ContentType)?> GenerarReporteAsync(int idRecepcionDocumento, string formato = "pdf");
}
