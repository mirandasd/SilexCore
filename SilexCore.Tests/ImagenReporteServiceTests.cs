using Microsoft.Extensions.Configuration;
using SilexCore.Infrastructure.Services;

namespace SilexCore.Tests;

// ImagenReporteService toca el sistema de archivos real (logos/fondos que se
// incrustan en los PDF de factura/recepción) -- no hay una abstracción de
// filesystem en el proyecto, así que se prueba con archivos temporales reales
// en vez de forzar un mock que no refleja el comportamiento real de Path/File.
public class ImagenReporteServiceTests : IDisposable
{
    private readonly string _rutaTemporal = Path.Combine(Path.GetTempPath(), "SilexCoreTests_" + Guid.NewGuid());

    public ImagenReporteServiceTests()
    {
        Directory.CreateDirectory(_rutaTemporal);
    }

    public void Dispose()
    {
        if (Directory.Exists(_rutaTemporal))
            Directory.Delete(_rutaTemporal, recursive: true);
    }

    private ImagenReporteService ServicioConRutaBase(string? rutaBase)
    {
        var configuracion = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["ImagenesReporteSettings:RutaBase"] = rutaBase })
            .Build();
        return new ImagenReporteService(configuracion);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task ObtenerComoDataUriAsync_SinNombreDeArchivo_DevuelveNull(string? nombreArchivo)
    {
        var resultado = await ServicioConRutaBase(_rutaTemporal).ObtenerComoDataUriAsync(nombreArchivo);
        Assert.Null(resultado);
    }

    [Fact]
    public async Task ObtenerComoDataUriAsync_SinRutaBaseConfigurada_DevuelveNull()
    {
        var resultado = await ServicioConRutaBase(null).ObtenerComoDataUriAsync("logo.png");
        Assert.Null(resultado);
    }

    [Fact]
    public async Task ObtenerComoDataUriAsync_CuandoElArchivoNoExiste_DevuelveNull()
    {
        var resultado = await ServicioConRutaBase(_rutaTemporal).ObtenerComoDataUriAsync("no-existe.png");
        Assert.Null(resultado);
    }

    [Theory]
    [InlineData("logo.png", "image/png")]
    [InlineData("logo.PNG", "image/png")] // extensión sin distinguir mayúsculas
    [InlineData("logo.jpg", "image/jpeg")]
    [InlineData("logo.jpeg", "image/jpeg")]
    [InlineData("logo.gif", "image/gif")]
    [InlineData("logo.bmp", "image/png")] // extensión desconocida -> por defecto png
    public async Task ObtenerComoDataUriAsync_ArmaElDataUriConElMimeSegunLaExtension(string nombreArchivo, string mimeEsperado)
    {
        var bytes = new byte[] { 1, 2, 3, 4 };
        await File.WriteAllBytesAsync(Path.Combine(_rutaTemporal, nombreArchivo), bytes);

        var resultado = await ServicioConRutaBase(_rutaTemporal).ObtenerComoDataUriAsync(nombreArchivo);

        Assert.Equal($"data:{mimeEsperado};base64,{Convert.ToBase64String(bytes)}", resultado);
    }
}
