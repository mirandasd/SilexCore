using Karin.InvoicingService.Client.Dtos;
using Karin.InvoicingService.Client.Interface;
using Moq;
using Refit;
using SilexCore.Infrastructure.Services;
using SilexCore.Tests.TestHelpers;

namespace SilexCore.Tests;

public class CertificadoConsultaServiceTests
{
    private readonly Mock<IInvoicingServiceClient> _invoicingClient = new();
    private readonly CertificadoConsultaService _service;

    public CertificadoConsultaServiceTests()
    {
        _service = new CertificadoConsultaService(_invoicingClient.Object);
    }

    [Fact]
    public async Task ObtenerEstadoCertificadosAsync_CuandoInvoicingServiceFalla_Lanza()
    {
        _invoicingClient
            .Setup(c => c.ObtenerEstadoCertificadosAsync())
            .ReturnsAsync(RefitResponseFactory.Fallo<List<EstadoCertificadoResponse>>());

        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.ObtenerEstadoCertificadosAsync());
    }

    [Fact]
    public async Task ObtenerEstadoCertificadosAsync_MapeaTodosLosCamposDeCadaCertificado()
    {
        var vencimiento = new DateTime(2027, 1, 1);
        _invoicingClient
            .Setup(c => c.ObtenerEstadoCertificadosAsync())
            .ReturnsAsync(RefitResponseFactory.Ok(new List<EstadoCertificadoResponse>
            {
                new()
                {
                    IdEntidad = 1,
                    NombreComercial = "QUEBRADOR Y MATERIALES MANFRED",
                    TieneCertificadoRegistrado = true,
                    ArchivoEncontrado = true,
                    FechaVencimiento = vencimiento,
                    DiasRestantes = 100,
                    Vencido = false,
                    Mensaje = "OK"
                }
            }));

        var resultado = await _service.ObtenerEstadoCertificadosAsync();

        var item = Assert.Single(resultado);
        Assert.Equal(1, item.IdEntidad);
        Assert.Equal("QUEBRADOR Y MATERIALES MANFRED", item.NombreComercial);
        Assert.True(item.TieneCertificadoRegistrado);
        Assert.True(item.ArchivoEncontrado);
        Assert.Equal(vencimiento, item.FechaVencimiento);
        Assert.Equal(100, item.DiasRestantes);
        Assert.False(item.Vencido);
        Assert.Equal("OK", item.Mensaje);
    }

    [Fact]
    public async Task SubirCertificadoAsync_CuandoInvoicingServiceFalla_Lanza()
    {
        _invoicingClient
            .Setup(c => c.SubirCertificadoAsync(It.IsAny<int>(), It.IsAny<StreamPart>(), It.IsAny<string>()))
            .ReturnsAsync(RefitResponseFactory.Fallo<GuardarCertificadoResponse>());

        using var stream = new MemoryStream();
        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.SubirCertificadoAsync(1, stream, "cert.p12", "clave"));
    }

    [Fact]
    public async Task SubirCertificadoAsync_DevuelveLosDatosDelCertificadoGuardado()
    {
        var vencimiento = new DateTime(2028, 6, 1);
        _invoicingClient
            .Setup(c => c.SubirCertificadoAsync(1, It.IsAny<StreamPart>(), "clave-secreta"))
            .ReturnsAsync(RefitResponseFactory.Ok(new GuardarCertificadoResponse
            {
                IdEntidad = 1,
                NombreArchivo = "cert.p12",
                FechaVencimiento = vencimiento,
                Mensaje = "Certificado guardado"
            }));

        using var stream = new MemoryStream();
        var resultado = await _service.SubirCertificadoAsync(1, stream, "cert.p12", "clave-secreta");

        Assert.Equal(1, resultado.IdEntidad);
        Assert.Equal("cert.p12", resultado.NombreArchivo);
        Assert.Equal(vencimiento, resultado.FechaVencimiento);
        Assert.Equal("Certificado guardado", resultado.Mensaje);
    }
}
