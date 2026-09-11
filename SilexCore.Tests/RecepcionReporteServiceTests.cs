using Moq;
using SilexCore.Domain.Dtos;
using SilexCore.Domain.Interfaces.Infraestructura.Persistence;
using SilexCore.Domain.Interfaces.Infraestructura.Services;
using SilexCore.Infrastructure.Services;
using SilexCore.Tests.TestHelpers;

namespace SilexCore.Tests;

public class RecepcionReporteServiceTests
{
    private readonly Mock<IRecepcionRepository> _recepcionRepo = new();
    private readonly Mock<IUsuarioRepository> _usuarioRepo = new();
    private readonly Mock<IImagenReporteService> _imagenes = new();
    private readonly Mock<IReporteService> _reportes = new();
    private readonly RecepcionReporteService _service;

    public RecepcionReporteServiceTests()
    {
        _service = new RecepcionReporteService(_recepcionRepo.Object, _usuarioRepo.Object, _imagenes.Object, _reportes.Object);
    }

    [Fact]
    public async Task GenerarReporteAsync_CuandoLaRecepcionNoExiste_DevuelveNullSinConsultarNadaMas()
    {
        _recepcionRepo.Setup(r => r.ObtenerPorIdAsync(99)).ReturnsAsync((RecepcionDetalleResponse?)null);

        var resultado = await _service.GenerarReporteAsync(99);

        Assert.Null(resultado);
        _usuarioRepo.Verify(u => u.ObtenerEntidadReporteAsync(It.IsAny<int>()), Times.Never);
        _reportes.Verify(r => r.GenerarReporteAsync(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task GenerarReporteAsync_FormateaLosTotalesYElMontoEnLetrasConLaMonedaDeLaRecepcion()
    {
        _recepcionRepo.Setup(r => r.ObtenerPorIdAsync(1)).ReturnsAsync(new RecepcionDetalleResponse
        {
            IdEntidad = 1,
            Impuesto = 130m,
            ImpuestoAcreditar = 100m,
            GastoAplicable = 30m,
            Total = 1130m,
            Moneda = "Dolares"
        });

        object? datosCapturados = null;
        _reportes
            .Setup(r => r.GenerarReporteAsync("Recepcion", It.IsAny<object>(), "pdf"))
            .Callback<string, object, string>((_, datos, _) => datosCapturados = datos)
            .ReturnsAsync(([], "application/pdf"));

        await _service.GenerarReporteAsync(1);

        var totales = AnonymousObjectAssert.Prop(datosCapturados, "totales");
        Assert.Equal("130.00", AnonymousObjectAssert.Prop(totales, "impuesto"));
        Assert.Equal("100.00", AnonymousObjectAssert.Prop(totales, "impuesto_acreditar"));
        Assert.Equal("30.00", AnonymousObjectAssert.Prop(totales, "gasto_aplicable"));
        Assert.Equal("1,130.00", AnonymousObjectAssert.Prop(totales, "total"));
        Assert.Equal("MIL CIENTO TREINTA DOLARES", AnonymousObjectAssert.Prop(datosCapturados, "monto_en_letras"));
    }
}
