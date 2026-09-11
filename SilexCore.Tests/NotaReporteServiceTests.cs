using Moq;
using SilexCore.Domain.Dtos;
using SilexCore.Domain.Interfaces.Infraestructura.Persistence;
using SilexCore.Domain.Interfaces.Infraestructura.Services;
using SilexCore.Infrastructure.Services;
using SilexCore.Tests.TestHelpers;

namespace SilexCore.Tests;

// El cálculo de totales es el mismo que en FacturaReporteServiceTests (código
// duplicado a propósito, no una reimplementación distinta) -- lo que sí es
// propio de Nota, y lo único que se prueba aquí, es que el detalle y el
// cliente se resuelven a través de la FACTURA referenciada (tbl_nota no trae
// su propio detalle/cliente), y los campos exclusivos de nota (acción, razón,
// referencia a la factura original).
public class NotaReporteServiceTests
{
    private readonly Mock<INotaRepository> _notaRepo = new();
    private readonly Mock<IFacturaRepository> _facturaRepo = new();
    private readonly Mock<IClienteRepository> _clienteRepo = new();
    private readonly Mock<IUsuarioRepository> _usuarioRepo = new();
    private readonly Mock<IImagenReporteService> _imagenes = new();
    private readonly Mock<IReporteService> _reportes = new();
    private readonly NotaReporteService _service;

    public NotaReporteServiceTests()
    {
        _service = new NotaReporteService(_notaRepo.Object, _facturaRepo.Object, _clienteRepo.Object, _usuarioRepo.Object, _imagenes.Object, _reportes.Object);
        _facturaRepo.Setup(r => r.ObtenerDetalleFacturaPorIdAsync(It.IsAny<int>())).ReturnsAsync([]);
    }

    [Fact]
    public async Task GenerarReporteAsync_CuandoLaNotaNoExiste_DevuelveNullSinConsultarNadaMas()
    {
        _notaRepo.Setup(r => r.ObtenerNotaPorIdAsync(99)).ReturnsAsync((NotaResponse?)null);

        var resultado = await _service.GenerarReporteAsync(99);

        Assert.Null(resultado);
        _facturaRepo.Verify(r => r.ObtenerDetalleFacturaPorIdAsync(It.IsAny<int>()), Times.Never);
        _reportes.Verify(r => r.GenerarReporteAsync(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task GenerarReporteAsync_ConsultaElDetalleUsandoElIdFacturaDeLaNota_NoElIdNota()
    {
        _notaRepo.Setup(r => r.ObtenerNotaPorIdAsync(5)).ReturnsAsync(new NotaResponse { IdFactura = 500, IdCliente = 10, IdEntidad = 1 });

        await _service.GenerarReporteAsync(5);

        _facturaRepo.Verify(r => r.ObtenerDetalleFacturaPorIdAsync(500), Times.Once);
        _facturaRepo.Verify(r => r.ObtenerDetalleFacturaPorIdAsync(5), Times.Never);
    }

    [Fact]
    public async Task GenerarReporteAsync_ConsultaElClienteUsandoElIdClienteDeLaNota()
    {
        _notaRepo.Setup(r => r.ObtenerNotaPorIdAsync(5)).ReturnsAsync(new NotaResponse { IdFactura = 500, IdCliente = 10, IdEntidad = 1 });

        await _service.GenerarReporteAsync(5);

        _clienteRepo.Verify(c => c.ObtenerClientePorIdAsync(10), Times.Once);
    }

    [Fact]
    public async Task GenerarReporteAsync_IncluyeAccionRazonYLaReferenciaALaFacturaOriginal()
    {
        _notaRepo.Setup(r => r.ObtenerNotaPorIdAsync(5)).ReturnsAsync(new NotaResponse
        {
            IdFactura = 500,
            IdCliente = 10,
            IdEntidad = 1,
            Accion = "Anular documento de referencia",
            Razon = "Error en el monto",
            FacturaConsecutivo = "00100001010000000001",
            FacturaClave = "50601..."
        });

        object? datosCapturados = null;
        _reportes
            .Setup(r => r.GenerarReporteAsync("Nota", It.IsAny<object>(), "pdf"))
            .Callback<string, object, string>((_, datos, _) => datosCapturados = datos)
            .ReturnsAsync(([], "application/pdf"));

        await _service.GenerarReporteAsync(5);

        var documento = AnonymousObjectAssert.Prop(datosCapturados, "documento");
        Assert.Equal("Anular documento de referencia", AnonymousObjectAssert.Prop(documento, "accion"));
        Assert.Equal("Error en el monto", AnonymousObjectAssert.Prop(documento, "razon"));
        Assert.Equal("00100001010000000001", AnonymousObjectAssert.Prop(documento, "referencia_consecutivo"));
        Assert.Equal("50601...", AnonymousObjectAssert.Prop(documento, "referencia_clave"));
    }
}
