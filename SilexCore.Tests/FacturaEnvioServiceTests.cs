using Karin.InvoicingService.Client.Dtos;
using Karin.InvoicingService.Client.Enums;
using Karin.InvoicingService.Client.Interface;
using Moq;
using SilexCore.Domain.Interfaces.Infraestructura.Persistence;
using SilexCore.Infrastructure.Services;
using SilexCore.Tests.TestHelpers;

namespace SilexCore.Tests;

public class FacturaEnvioServiceTests
{
    private readonly Mock<IInvoicingServiceClient> _invoicingClient = new();
    private readonly Mock<IFacturaRepository> _facturaRepo = new();
    private readonly FacturaEnvioService _service;

    public FacturaEnvioServiceTests()
    {
        _service = new FacturaEnvioService(_invoicingClient.Object, _facturaRepo.Object);
        _facturaRepo
            .Setup(r => r.GuardarHistoricoFacturaAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync((1, "ok"));
    }

    private void SetupRespuesta(ComprobanteResponse contenido) =>
        _invoicingClient
            .Setup(c => c.EnviarComprobanteAsync(TipoComprobante.Factura, It.IsAny<ComprobanteRequest>()))
            .ReturnsAsync(RefitResponseFactory.Ok(contenido));

    [Fact]
    public async Task EnviarFacturaAsync_CuandoInvoicingServiceFalla_Lanza()
    {
        _invoicingClient
            .Setup(c => c.EnviarComprobanteAsync(TipoComprobante.Factura, It.IsAny<ComprobanteRequest>()))
            .ReturnsAsync(RefitResponseFactory.Fallo<ComprobanteResponse>());

        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.EnviarFacturaAsync(1));
    }

    [Theory]
    [InlineData("aceptado", 1)]
    [InlineData("ACEPTADO", 1)] // MapearEstadoHacienda normaliza mayúsculas
    [InlineData(" aceptado ", 1)] // y espacios
    [InlineData("rechazado", 3)]
    [InlineData("procesando", 2)] // cualquier otro valor -> "Pendiente" (2)
    [InlineData("ErrorEnvio", 2)]
    public async Task EnviarFacturaAsync_MapeaElEstadoDeHaciendaAlCodigoEsperado(string estado, int estadoHaciendaEsperado)
    {
        SetupRespuesta(new ComprobanteResponse { Estado = estado, Clave = "clave-1", Consecutivo = "c-1" });

        await _service.EnviarFacturaAsync(10);

        _facturaRepo.Verify(r => r.GuardarHistoricoFacturaAsync(10, "clave-1", It.IsAny<string>(), string.Empty, It.IsAny<string>(), It.IsAny<int>(), estadoHaciendaEsperado), Times.Once);
    }

    [Fact]
    public async Task EnviarFacturaAsync_CuandoElEstadoEsErrorEnvio_GuardaEstadoEnvioEnCero()
    {
        SetupRespuesta(new ComprobanteResponse { Estado = "ErrorEnvio" });

        await _service.EnviarFacturaAsync(10);

        _facturaRepo.Verify(r => r.GuardarHistoricoFacturaAsync(10, It.IsAny<string>(), It.IsAny<string>(), string.Empty, It.IsAny<string>(), 0, It.IsAny<int>()), Times.Once);
    }

    [Fact]
    public async Task EnviarFacturaAsync_CuandoElEstadoNoEsErrorEnvio_GuardaEstadoEnvioEnUno()
    {
        SetupRespuesta(new ComprobanteResponse { Estado = "aceptado" });

        await _service.EnviarFacturaAsync(10);

        _facturaRepo.Verify(r => r.GuardarHistoricoFacturaAsync(10, It.IsAny<string>(), It.IsAny<string>(), string.Empty, It.IsAny<string>(), 1, It.IsAny<int>()), Times.Once);
    }

    [Fact]
    public async Task EnviarFacturaAsync_CuandoNoSePuedeGuardarElHistorico_Lanza()
    {
        SetupRespuesta(new ComprobanteResponse { Estado = "aceptado" });
        _facturaRepo
            .Setup(r => r.GuardarHistoricoFacturaAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync((0, "Error de base de datos"));

        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.EnviarFacturaAsync(10));
    }

    [Fact]
    public async Task EnviarFacturaAsync_DevuelveLosDatosDelComprobanteRecibido()
    {
        SetupRespuesta(new ComprobanteResponse
        {
            Estado = "aceptado",
            Clave = "50601...",
            Consecutivo = "00100001010000000001",
            Mensaje = "Comprobante aceptado"
        });

        var resultado = await _service.EnviarFacturaAsync(10);

        Assert.Equal("50601...", resultado.Clave);
        Assert.Equal("00100001010000000001", resultado.Consecutivo);
        Assert.Equal("aceptado", resultado.Estado);
        Assert.Equal("Comprobante aceptado", resultado.Mensaje);
    }
}
