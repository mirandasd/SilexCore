using Microsoft.Extensions.Configuration;
using SilexCore.Domain.Interfaces.Infraestructura.Services;

namespace SilexCore.Infrastructure.Services;

public class ImagenReporteService(IConfiguration configuration) : IImagenReporteService
{
    public async Task<string?> ObtenerComoDataUriAsync(string? nombreArchivo)
    {
        if (string.IsNullOrWhiteSpace(nombreArchivo)) return null;

        var rutaBase = configuration["ImagenesReporteSettings:RutaBase"];
        if (string.IsNullOrWhiteSpace(rutaBase)) return null;

        var rutaCompleta = Path.Combine(rutaBase, nombreArchivo);
        if (!File.Exists(rutaCompleta)) return null;

        var bytes = await File.ReadAllBytesAsync(rutaCompleta);
        var mime = Path.GetExtension(nombreArchivo).ToLowerInvariant() switch
        {
            ".png" => "image/png",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".gif" => "image/gif",
            _ => "image/png"
        };

        return $"data:{mime};base64,{Convert.ToBase64String(bytes)}";
    }
}
