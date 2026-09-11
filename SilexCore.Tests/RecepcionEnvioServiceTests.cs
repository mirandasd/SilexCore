using Karin.InvoicingService.Client.Dtos;
using Karin.InvoicingService.Client.Enums;
using Karin.InvoicingService.Client.Interface;
using Moq;
using SilexCore.Domain.Interfaces.Infraestructura.Persistence;
using SilexCore.Infrastructure.Services;
using SilexCore.Tests.TestHelpers;

namespace SilexCore.Tests;

public class RecepcionEnvioServiceTests
{
    private readonly Mock<IInvoicingServiceClient> _invoicingClient = new();
    private readonly Mock<IRecepcionRepository> _recepcionRepo = new();
    private readonly RecepcionEnvioService _service;

    public RecepcionEnvioServiceTests()
    {
        _service = new RecepcionEnvioService(_invoicingClient.Object, _recepcionRepo.Object);
        _recepcionRepo
            .Setup(r => r.GuardarHistoricoRecepcionAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync((1, "ok"));
    }

    [Fact]
    public async Task EnviarRecepcionAsync_UsaElTipoMensajeReceptor()
    {
        _invoicingClient
            .Setup(c => c.EnviarComprobanteAsync(TipoComprobante.MensajeReceptor, It.IsAny<ComprobanteRequest>()))
            .ReturnsAsync(RefitResponseFactory.Ok(new ComprobanteResponse { Estado = "aceptado", Consecutivo = "consec-1" }));

        await _service.EnviarRecepcionAsync(7);

        _invoicingClient.Verify(c => c.EnviarComprobanteAsync(TipoComprobante.MensajeReceptor,
            It.Is<ComprobanteRequest>(r => r.IdDocumento == 7)), Times.Once);
    }

    [Fact]
    public async Task EnviarRecepcionAsync_GuardaElConsecutivoEnElHistorico_NoLaClave()
    {
        // A diferencia de Factura/Nota: el histórico de Recepción identifica el
        // documento por Consecutivo (el mensaje receptor propio), no por la Clave
        // (que es la del documento del emisor que se está confirmando).
        _invoicingClient
            .Setup(c => c.EnviarComprobanteAsync(TipoComprobante.MensajeReceptor, It.IsAny<ComprobanteRequest>()))
            .ReturnsAsync(RefitResponseFactory.Ok(new ComprobanteResponse { Estado = "aceptado", Clave = "clave-del-emisor", Consecutivo = "consec-propio" }));

        await _service.EnviarRecepcionAsync(7);

        _recepcionRepo.Verify(r => r.GuardarHistoricoRecepcionAsync(7, "consec-propio", It.IsAny<string>(), string.Empty, It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()), Times.Once);
        _recepcionRepo.Verify(r => r.GuardarHistoricoRecepcionAsync(7, "clave-del-emisor", It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task EnviarRecepcionAsync_CuandoInvoicingServiceFalla_Lanza()
    {
        _invoicingClient
            .Setup(c => c.EnviarComprobanteAsync(TipoComprobante.MensajeReceptor, It.IsAny<ComprobanteRequest>()))
            .ReturnsAsync(RefitResponseFactory.Fallo<ComprobanteResponse>());

        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.EnviarRecepcionAsync(7));
    }

    [Fact]
    public async Task EnviarRecepcionAsync_CuandoNoSePuedeGuardarElHistorico_Lanza()
    {
        _invoicingClient
            .Setup(c => c.EnviarComprobanteAsync(TipoComprobante.MensajeReceptor, It.IsAny<ComprobanteRequest>()))
            .ReturnsAsync(RefitResponseFactory.Ok(new ComprobanteResponse { Estado = "aceptado" }));
        _recepcionRepo
            .Setup(r => r.GuardarHistoricoRecepcionAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync((0, "Error de base de datos"));

        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.EnviarRecepcionAsync(7));
    }
}
