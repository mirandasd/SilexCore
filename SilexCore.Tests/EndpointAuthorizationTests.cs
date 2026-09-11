using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using SilexCore.Api.Endpoints.v1;
using SilexCore.Api.Interfaces;
using SilexCore.Domain.Interfaces.Infraestructura.Persistence;
using SilexCore.Domain.Interfaces.Infraestructura.Services;

namespace SilexCore.Tests;

// Cubre la auditoría de seguridad del 2026-09-10: Factura, Cliente, Consecutivo
// y Hacienda estaban completamente abiertos (sin RequireAuthorization), y
// GET /api/certificados/estado no tenía ningún guard de rol. Estos endpoints
// se registran con MapGroup/MapGet directamente sobre IEndpointRouteBuilder,
// así que la única forma real de probarlos es construir la app y leer los
// metadatos de cada endpoint -- leer el código fuente a ojo es exactamente
// lo que ya falló una vez (CertificadoEndpoints se armó sin auth ninguna).
// RequireAuthorization() solo agrega metadata IAuthorizeData al mapear -- no
// hace falta registrar el pipeline real de autenticación/autorización para
// leerla, por eso cada prueba arma un WebApplication mínimo con solo los
// servicios que ese módulo de endpoints necesita para poder resolverse.
public class EndpointAuthorizationTests
{
    private static IReadOnlyList<RouteEndpoint> EndpointsDe(WebApplication app) =>
        ((IEndpointRouteBuilder)app).DataSources
            .SelectMany(ds => ds.Endpoints)
            .OfType<RouteEndpoint>()
            .ToList();

    private static RouteEndpoint EndpointDe(IReadOnlyList<RouteEndpoint> endpoints, string metodoHttp, string patronRuta)
    {
        var encontrado = endpoints.SingleOrDefault(e =>
            string.Equals(e.RoutePattern.RawText, patronRuta, StringComparison.OrdinalIgnoreCase)
            && (e.Metadata.GetMetadata<IHttpMethodMetadata>()?.HttpMethods.Contains(metodoHttp) ?? false));

        Assert.True(encontrado is not null, $"No se encontró el endpoint {metodoHttp} {patronRuta}. Rutas registradas: " +
            string.Join(", ", endpoints.Select(e => $"{e.Metadata.GetMetadata<IHttpMethodMetadata>()?.HttpMethods.FirstOrDefault()} {e.RoutePattern.RawText}")));
        return encontrado!;
    }

    private static bool RequiereAutenticacion(RouteEndpoint endpoint) =>
        endpoint.Metadata.GetMetadata<IAuthorizeData>() is not null
        && endpoint.Metadata.GetMetadata<IAllowAnonymous>() is null;

    private static IEnumerable<string?> PoliticasDe(RouteEndpoint endpoint) =>
        endpoint.Metadata.GetOrderedMetadata<IAuthorizeData>().Select(a => a.Policy);

    [Theory]
    [InlineData("GET", "/api/factura/negocio/{idEntidad:int}")]
    [InlineData("POST", "/api/factura/")]
    public void GrupoFactura_RequiereAutenticacion(string metodo, string ruta)
    {
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddSingleton(Mock.Of<IFacturaRepository>());
        builder.Services.AddSingleton(Mock.Of<IBitacoraService>());
        builder.Services.AddSingleton(Mock.Of<IFacturaEnvioService>());
        builder.Services.AddSingleton(Mock.Of<IFacturaReporteService>());
        var app = builder.Build();
        new FacturaEndpoints().RegistrarEndpoints(app);

        var endpoints = EndpointsDe(app);
        Assert.True(RequiereAutenticacion(EndpointDe(endpoints, metodo, ruta)));
    }

    [Theory]
    [InlineData("GET", "/api/cliente/")]
    [InlineData("POST", "/api/cliente/")]
    [InlineData("DELETE", "/api/cliente/{idCliente:int}")]
    public void GrupoCliente_RequiereAutenticacion(string metodo, string ruta)
    {
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddSingleton(Mock.Of<IClienteRepository>());
        var app = builder.Build();
        new ClienteEndpoints().RegistrarEndpoints(app);

        var endpoints = EndpointsDe(app);
        Assert.True(RequiereAutenticacion(EndpointDe(endpoints, metodo, ruta)));
    }

    [Theory]
    [InlineData("GET", "/api/consecutivo/negocio/{idEntidad:int}")]
    [InlineData("PUT", "/api/consecutivo/{idConsecutivo:int}")]
    public void GrupoConsecutivo_RequiereAutenticacion(string metodo, string ruta)
    {
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddSingleton(Mock.Of<IConsecutivoRepository>());
        var app = builder.Build();
        new ConsecutivoEndpoints().RegistrarEndpoints(app);

        var endpoints = EndpointsDe(app);
        Assert.True(RequiereAutenticacion(EndpointDe(endpoints, metodo, ruta)));
    }

    [Theory]
    [InlineData("GET", "/api/hacienda/monedas")]
    [InlineData("GET", "/api/hacienda/cabys")]
    public void GrupoHacienda_RequiereAutenticacion(string metodo, string ruta)
    {
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddSingleton(Mock.Of<IHaciendaCatalogServices>());
        var app = builder.Build();
        new HaciendaCatalogEndpoints().RegistrarEndpoints(app);

        var endpoints = EndpointsDe(app);
        Assert.True(RequiereAutenticacion(EndpointDe(endpoints, metodo, ruta)));
    }

    [Fact]
    public void ObtenerEstadoCertificados_RequierePolicyDeAdmin()
    {
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddSingleton(Mock.Of<ICertificadoConsultaService>());
        var app = builder.Build();
        new CertificadoEndpoints().RegistrarEndpoints(app);

        var endpoints = EndpointsDe(app);
        var endpoint = EndpointDe(endpoints, "GET", "/api/certificados/estado");

        Assert.Contains("RequiereRolAdmin", PoliticasDe(endpoint));
    }

    [Fact]
    public void SubirCertificado_RequierePolicyDeAdmin()
    {
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddSingleton(Mock.Of<ICertificadoConsultaService>());
        var app = builder.Build();
        new CertificadoEndpoints().RegistrarEndpoints(app);

        var endpoints = EndpointsDe(app);
        var endpoint = EndpointDe(endpoints, "POST", "/api/certificados/{idEntidad:int}");

        Assert.Contains("RequiereRolAdmin", PoliticasDe(endpoint));
    }

    [Fact]
    public void RegistrarUsuario_RequierePolicyDeAdmin()
    {
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddSingleton(Mock.Of<IAutenticacionService>());
        builder.Services.AddSingleton(Mock.Of<IUsuarioRepository>());
        builder.Services.AddSingleton(Mock.Of<IBitacoraService>());
        var app = builder.Build();
        new UsuarioEndpoints().RegistrarEndpoints(app);

        var endpoints = EndpointsDe(app);
        var endpoint = EndpointDe(endpoints, "POST", "/api/usuario/registro");

        Assert.Contains("RequiereRolAdmin", PoliticasDe(endpoint));
    }
}
