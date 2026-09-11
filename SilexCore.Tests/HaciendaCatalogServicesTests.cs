using System.Net;
using Karin.HaciendaCatalogService.Client.Interface;
using Moq;
using Refit;
using SilexCore.Infrastructure.Services;

namespace SilexCore.Tests;

public class HaciendaCatalogServicesTests
{
    private readonly Mock<IHaciendaCatalogServiceClient> _client = new();
    private readonly HaciendaCatalogServices _service;

    public HaciendaCatalogServicesTests()
    {
        _service = new HaciendaCatalogServices(_client.Object);
    }

    private static Task<ApiException> ExcepcionCon(HttpStatusCode codigo) => ApiException.Create(
        new HttpRequestMessage(HttpMethod.Get, "http://localhost/fe/ex"),
        HttpMethod.Get,
        new HttpResponseMessage(codigo),
        new RefitSettings());

    [Fact]
    public async Task ObtenerExoneracionAsync_CuandoHaciendaResponde404_DevuelveNullEnVezDePropagarLaExcepcion()
    {
        // La API real de Hacienda usa 404 para "esta autorización no existe" --
        // es un resultado válido de la búsqueda, no un error del sistema.
        _client.Setup(c => c.GetExoneracionAsync("no-existe")).ThrowsAsync(await ExcepcionCon(HttpStatusCode.NotFound));

        var resultado = await _service.ObtenerExoneracionAsync("no-existe");

        Assert.Null(resultado);
    }

    [Fact]
    public async Task ObtenerExoneracionAsync_CuandoElErrorNoEs404_PropagaLaExcepcion()
    {
        _client.Setup(c => c.GetExoneracionAsync("falla")).ThrowsAsync(await ExcepcionCon(HttpStatusCode.InternalServerError));

        await Assert.ThrowsAsync<ApiException>(() => _service.ObtenerExoneracionAsync("falla"));
    }

    [Fact]
    public async Task ObtenerExoneracionAsync_CuandoSiExiste_MapeaLosCamposPrincipales()
    {
        _client.Setup(c => c.GetExoneracionAsync("123")).ReturnsAsync(new Karin.HaciendaCatalogService.Client.Dtos.ExoneracionResponse
        {
            NumeroDocumento = "DOC-1",
            Identificacion = "3101123456",
            Autorizacion = 123,
            PorcentajeExoneracion = 100m
        });

        var resultado = await _service.ObtenerExoneracionAsync("123");

        Assert.NotNull(resultado);
        Assert.Equal("DOC-1", resultado!.NumeroDocumento);
        Assert.Equal("3101123456", resultado.Identificacion);
        Assert.Equal(123, resultado.Autorizacion);
        Assert.Equal(100m, resultado.PorcentajeExoneracion);
    }

    [Fact]
    public async Task ObtenerContribuyenteAsync_CuandoElClienteDevuelveNull_DevuelveNull()
    {
        _client.Setup(c => c.ObtenerContribuyenteAsync("0")).ReturnsAsync((Karin.HaciendaCatalogService.Client.Dtos.ContribuyenteResponse?)null);

        var resultado = await _service.ObtenerContribuyenteAsync("0");

        Assert.Null(resultado);
    }

    [Fact]
    public async Task ObtenerProvinciasAsync_MapeaCadaProvinciaCorrectamente()
    {
        _client.Setup(c => c.ObtenerProvinciasAsync()).ReturnsAsync(new List<Karin.HaciendaCatalogService.Client.Dtos.ProvinciaResponse>
        {
            new() { IdProvincia = 1, Provincia = "San José" }
        });

        var resultado = await _service.ObtenerProvinciasAsync();

        var item = Assert.Single(resultado);
        Assert.Equal(1, item.IdProvincia);
        Assert.Equal("San José", item.Provincia);
    }
}
