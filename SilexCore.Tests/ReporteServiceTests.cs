using System.Net;
using System.Net.Http.Headers;
using Karin.ReportingService.Client.Dtos;
using Karin.ReportingService.Client.Interface;
using Moq;
using SilexCore.Infrastructure.Services;

namespace SilexCore.Tests;

public class ReporteServiceTests
{
    private readonly Mock<IReportingServiceClient> _client = new();
    private readonly ReporteService _service;

    public ReporteServiceTests()
    {
        _service = new ReporteService(_client.Object);
    }

    [Fact]
    public async Task GenerarReporteAsync_DevuelveElContenidoYElContentTypeDeLaRespuesta()
    {
        var bytes = new byte[] { 1, 2, 3 };
        var respuesta = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new ByteArrayContent(bytes)
        };
        respuesta.Content.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
        _client.Setup(c => c.GenerarReporteAsync(It.IsAny<GenerarReporteRequest>())).ReturnsAsync(respuesta);

        var (contenido, contentType) = await _service.GenerarReporteAsync("Factura", new { });

        Assert.Equal(bytes, contenido);
        Assert.Equal("application/pdf", contentType);
    }

    [Fact]
    public async Task GenerarReporteAsync_CuandoElContentTypeNoViene_UsaOctetStreamPorDefecto()
    {
        var respuesta = new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent([1]) };
        _client.Setup(c => c.GenerarReporteAsync(It.IsAny<GenerarReporteRequest>())).ReturnsAsync(respuesta);

        var (_, contentType) = await _service.GenerarReporteAsync("Factura", new { });

        Assert.Equal("application/octet-stream", contentType);
    }

    [Fact]
    public async Task GenerarReporteAsync_CuandoReportingServiceFalla_Lanza()
    {
        var respuesta = new HttpResponseMessage(HttpStatusCode.InternalServerError) { Content = new ByteArrayContent([]) };
        _client.Setup(c => c.GenerarReporteAsync(It.IsAny<GenerarReporteRequest>())).ReturnsAsync(respuesta);

        await Assert.ThrowsAsync<HttpRequestException>(() => _service.GenerarReporteAsync("Factura", new { }));
    }

    [Fact]
    public async Task GenerarReporteAsync_EnviaElTipoLosDatosYElFormatoRecibidos()
    {
        var respuesta = new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent([1]) };
        _client.Setup(c => c.GenerarReporteAsync(It.IsAny<GenerarReporteRequest>())).ReturnsAsync(respuesta);
        var datos = new { Consecutivo = "001" };

        await _service.GenerarReporteAsync("Recepcion", datos, "html");

        _client.Verify(c => c.GenerarReporteAsync(It.Is<GenerarReporteRequest>(r =>
            r.TipoReporte == "Recepcion" && r.Formato == "html" && r.Datos == datos)), Times.Once);
    }
}
