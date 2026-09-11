using Moq;
using SilexCore.Domain.Dtos;
using SilexCore.Domain.Interfaces.Infraestructura.Persistence;
using SilexCore.Domain.Interfaces.Infraestructura.Services;
using SilexCore.Infrastructure.Services;
using SilexCore.Tests.TestHelpers;

namespace SilexCore.Tests;

// Los totales de esta factura son el cálculo que termina impreso en un
// documento tributario real -- si el prorrateo de exonerado/gravado se rompe,
// el PDF sale con el impuesto mal repartido sin que nada más lo detecte
// (ReportingService solo maqueta lo que se le manda, no valida montos).
public class FacturaReporteServiceTests
{
    private readonly Mock<IFacturaRepository> _facturaRepo = new();
    private readonly Mock<IClienteRepository> _clienteRepo = new();
    private readonly Mock<IUsuarioRepository> _usuarioRepo = new();
    private readonly Mock<IImagenReporteService> _imagenes = new();
    private readonly Mock<IReporteService> _reportes = new();
    private readonly FacturaReporteService _service;

    public FacturaReporteServiceTests()
    {
        _service = new FacturaReporteService(_facturaRepo.Object, _clienteRepo.Object, _usuarioRepo.Object, _imagenes.Object, _reportes.Object);
        _reportes
            .Setup(r => r.GenerarReporteAsync(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string>()))
            .ReturnsAsync(([], "application/pdf"));
    }

    private static FacturaResponse Factura(int idCliente = 1, int idEntidad = 1, decimal pagoCliente = 0, decimal vueltoCliente = 0, string moneda = "Colones") => new()
    {
        IdCliente = idCliente,
        IdEntidad = idEntidad,
        Consecutivo = "00100001010000000001",
        Moneda = moneda,
        PagoCliente = pagoCliente,
        VueltoCliente = vueltoCliente
    };

    private void SetupFactura(FacturaResponse factura, params DetalleFacturaResponse[] detalle)
    {
        _facturaRepo.Setup(r => r.ObtenerFacturaPorIdAsync(1)).ReturnsAsync(factura);
        _facturaRepo.Setup(r => r.ObtenerDetalleFacturaPorIdAsync(1)).ReturnsAsync(detalle);
    }

    [Fact]
    public async Task GenerarReporteAsync_CuandoLaFacturaNoExiste_DevuelveNullSinConsultarNadaMas()
    {
        _facturaRepo.Setup(r => r.ObtenerFacturaPorIdAsync(99)).ReturnsAsync((FacturaResponse?)null);

        var resultado = await _service.GenerarReporteAsync(99);

        Assert.Null(resultado);
        _clienteRepo.Verify(c => c.ObtenerClientePorIdAsync(It.IsAny<int>()), Times.Never);
        _reportes.Verify(r => r.GenerarReporteAsync(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task GenerarReporteAsync_UnaLineaSinExoneracion_TodoElImpuestoQuedaComoGravado()
    {
        SetupFactura(Factura(), new DetalleFacturaResponse
        {
            IdExoneracion = 0,
            Cantidad = 2,
            Precio = 100m,
            PorcentajeDescuento = 0,
            ValorTarifa = 13
        });

        object? datosCapturados = null;
        _reportes
            .Setup(r => r.GenerarReporteAsync("Factura", It.IsAny<object>(), "pdf"))
            .Callback<string, object, string>((_, datos, _) => datosCapturados = datos)
            .ReturnsAsync(([], "application/pdf"));

        await _service.GenerarReporteAsync(1);

        // subtotal = 2*100 = 200; sin descuento; impuesto = 200*13% = 26 (todo gravado)
        Assert.NotNull(datosCapturados);
        var totales = AnonymousObjectAssert.Prop(datosCapturados, "totales");
        Assert.Equal("200.00", AnonymousObjectAssert.Prop(totales, "subtotal"));
        Assert.Equal("26.00", AnonymousObjectAssert.Prop(totales, "gravado"));
        Assert.Equal("0.00", AnonymousObjectAssert.Prop(totales, "exonerado"));
        Assert.Equal("226.00", AnonymousObjectAssert.Prop(totales, "total"));
    }

    [Fact]
    public async Task GenerarReporteAsync_UnaLineaConExoneracion_TodoElImpuestoQuedaComoExonerado()
    {
        SetupFactura(Factura(), new DetalleFacturaResponse
        {
            IdExoneracion = 42, // > 0 => exonerada
            Cantidad = 1,
            Precio = 1000m,
            PorcentajeDescuento = 0,
            ValorTarifa = 13
        });

        object? datosCapturados = null;
        _reportes
            .Setup(r => r.GenerarReporteAsync("Factura", It.IsAny<object>(), "pdf"))
            .Callback<string, object, string>((_, datos, _) => datosCapturados = datos)
            .ReturnsAsync(([], "application/pdf"));

        await _service.GenerarReporteAsync(1);

        var totales = AnonymousObjectAssert.Prop(datosCapturados, "totales");
        Assert.Equal("0.00", AnonymousObjectAssert.Prop(totales, "gravado"));
        Assert.Equal("130.00", AnonymousObjectAssert.Prop(totales, "exonerado"));
        // El impuesto exonerado no se suma al total a pagar (impuesto_neto = solo gravado).
        Assert.Equal("1,000.00", AnonymousObjectAssert.Prop(totales, "total"));
    }

    [Fact]
    public async Task GenerarReporteAsync_AplicaElDescuentoDeLineaAntesDeCalcularElImpuesto()
    {
        SetupFactura(Factura(), new DetalleFacturaResponse
        {
            IdExoneracion = 0,
            Cantidad = 1,
            Precio = 100m,
            PorcentajeDescuento = 10, // 10% de descuento
            ValorTarifa = 13
        });

        object? datosCapturados = null;
        _reportes
            .Setup(r => r.GenerarReporteAsync("Factura", It.IsAny<object>(), "pdf"))
            .Callback<string, object, string>((_, datos, _) => datosCapturados = datos)
            .ReturnsAsync(([], "application/pdf"));

        await _service.GenerarReporteAsync(1);

        // subtotal=100, descuento=10, base=90, impuesto=90*13%=11.70
        var totales = AnonymousObjectAssert.Prop(datosCapturados, "totales");
        Assert.Equal("10.00", AnonymousObjectAssert.Prop(totales, "descuento"));
        Assert.Equal("90.00", AnonymousObjectAssert.Prop(totales, "venta_neta"));
        Assert.Equal("11.70", AnonymousObjectAssert.Prop(totales, "gravado"));
    }

    [Fact]
    public async Task GenerarReporteAsync_ElMontoEnLetrasUsaElTotalYLaMonedaDeLaFactura()
    {
        SetupFactura(Factura(moneda: "Dolares"), new DetalleFacturaResponse
        {
            Cantidad = 1,
            Precio = 100m,
            ValorTarifa = 0
        });

        object? datosCapturados = null;
        _reportes
            .Setup(r => r.GenerarReporteAsync("Factura", It.IsAny<object>(), "pdf"))
            .Callback<string, object, string>((_, datos, _) => datosCapturados = datos)
            .ReturnsAsync(([], "application/pdf"));

        await _service.GenerarReporteAsync(1);

        Assert.Equal("CIEN DOLARES", AnonymousObjectAssert.Prop(datosCapturados, "monto_en_letras"));
    }
}
