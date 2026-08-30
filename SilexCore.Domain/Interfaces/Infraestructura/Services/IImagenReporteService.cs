namespace SilexCore.Domain.Interfaces.Infraestructura.Services;

public interface IImagenReporteService
{
    // Devuelve un data URI (data:image/...;base64,...) listo para <img src="...">,
    // o null si el archivo no existe/no está configurado (el reporte debe poder
    // generarse igual sin logo/fondo).
    Task<string?> ObtenerComoDataUriAsync(string? nombreArchivo);
}
