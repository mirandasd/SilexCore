using Karin.InvoicingService.Client.Dtos;
using Karin.InvoicingService.Client.Enums;
using Karin.InvoicingService.Client.Interface;
using Moq;
using SilexCore.Domain.Interfaces.Infraestructura.Persistence;
using SilexCore.Infrastructure.Services;
using SilexCore.Tests.TestHelpers;

namespace SilexCore.Tests;

public class NotaEnvioServiceTests
{
    private readonly Mock<IInvoicingServiceClient> _invoicingClient = new();
    private readonly Mock<INotaRepository> _notaRepo = new();
    private readonly NotaEnvioService _service;

    public NotaEnvioServiceTests()
    {
        _service = new NotaEnvioService(_invoicingClient.Object, _notaRepo.Object);
        _notaRepo
            .Setup(r => r.GuardarHistoricoNotaAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync((1, "ok"));
    }

    [Fact]
    public async Task EnviarNotaAsync_UsaElTipoNotaCreditoAlLlamarAInvoicingService()
    {
        _invoicingClient
            .Setup(c => c.EnviarComprobanteAsync(TipoComprobante.NotaCredito, It.IsAny<ComprobanteRequest>()))
            .ReturnsAsync(RefitResponseFactory.Ok(new ComprobanteResponse { Estado = "aceptado", Clave = "clave-nota" }));

        await _service.EnviarNotaAsync(idNota: 5);

        _invoicingClient.Verify(c => c.EnviarComprobanteAsync(TipoComprobante.NotaCredito,
            It.Is<ComprobanteRequest>(r => r.IdDocumento == 5 && r.NuevoConsecutivo)), Times.Once);
    }

    [Fact]
    public async Task EnviarNotaAsync_CuandoInvoicingServiceFalla_Lanza()
    {
        _invoicingClient
            .Setup(c => c.EnviarComprobanteAsync(TipoComprobante.NotaCredito, It.IsAny<ComprobanteRequest>()))
            .ReturnsAsync(RefitResponseFactory.Fallo<ComprobanteResponse>());

        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.EnviarNotaAsync(5));
    }

    [Theory]
    [InlineData("aceptado", 1)]
    [InlineData("rechazado", 3)]
    [InlineData("procesando", 2)]
    public async Task EnviarNotaAsync_MapeaElEstadoDeHaciendaAlCodigoEsperado(string estado, int estadoHaciendaEsperado)
    {
        _invoicingClient
            .Setup(c => c.EnviarComprobanteAsync(TipoComprobante.NotaCredito, It.IsAny<ComprobanteRequest>()))
            .ReturnsAsync(RefitResponseFactory.Ok(new ComprobanteResponse { Estado = estado, Clave = "clave-nota" }));

        await _service.EnviarNotaAsync(5);

        _notaRepo.Verify(r => r.GuardarHistoricoNotaAsync(5, "clave-nota", It.IsAny<string>(), string.Empty, It.IsAny<string>(), It.IsAny<int>(), estadoHaciendaEsperado), Times.Once);
    }

    [Fact]
    public async Task EnviarNotaAsync_CuandoNoSePuedeGuardarElHistorico_Lanza()
    {
        _invoicingClient
            .Setup(c => c.EnviarComprobanteAsync(TipoComprobante.NotaCredito, It.IsAny<ComprobanteRequest>()))
            .ReturnsAsync(RefitResponseFactory.Ok(new ComprobanteResponse { Estado = "aceptado" }));
        _notaRepo
            .Setup(r => r.GuardarHistoricoNotaAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync((0, "Error de base de datos"));

        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.EnviarNotaAsync(5));
    }
}
