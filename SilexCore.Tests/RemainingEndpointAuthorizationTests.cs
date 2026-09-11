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

// Completa la cobertura de EndpointAuthorizationTests para el resto de los
// grupos de endpoints -- ya se habían revisado a mano en la auditoría del
// 2026-09-10 y estaban correctos, pero sin una prueba que lo fije no hay nada
// que avise si un futuro cambio les quita el RequireAuthorization por error.
// AuthEndpoints es el caso más delicado: el grupo en sí NO lleva auth (login,
// refresh-token, olvide/restablecer-password tienen que ser públicos), pero
// /me, /logout y /cambiar-password sí la exigen endpoint por endpoint.
public class RemainingEndpointAuthorizationTests
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

    [Theory]
    [InlineData("GET", "/api/catalogo/negocios")]
    [InlineData("GET", "/api/catalogo/categorias-venta")]
    [InlineData("GET", "/api/catalogo/opciones-venta/{idCategoria:int}")]
    public void GrupoCatalogo_RequiereAutenticacion(string metodo, string ruta)
    {
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddSingleton(Mock.Of<IUsuarioRepository>());
        builder.Services.AddSingleton(Mock.Of<IOpcionVentaRepository>());
        var app = builder.Build();
        new CatalogoEndpoints().RegistrarEndpoints(app);

        Assert.True(RequiereAutenticacion(EndpointDe(EndpointsDe(app), metodo, ruta)));
    }

    [Theory]
    [InlineData("GET", "/api/nota/negocio/{idEntidad:int}")]
    [InlineData("POST", "/api/nota/")]
    [InlineData("POST", "/api/nota/{idNota:int}/enviar")]
    public void GrupoNota_RequiereAutenticacion(string metodo, string ruta)
    {
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddSingleton(Mock.Of<INotaRepository>());
        builder.Services.AddSingleton(Mock.Of<IBitacoraService>());
        builder.Services.AddSingleton(Mock.Of<INotaReporteService>());
        builder.Services.AddSingleton(Mock.Of<INotaEnvioService>());
        var app = builder.Build();
        new NotaEndpoints().RegistrarEndpoints(app);

        Assert.True(RequiereAutenticacion(EndpointDe(EndpointsDe(app), metodo, ruta)));
    }

    [Theory]
    [InlineData("GET", "/api/produccion-material/negocio/{idEntidad:int}")]
    [InlineData("POST", "/api/produccion-material/")]
    [InlineData("PUT", "/api/produccion-material/")]
    public void GrupoProduccionMaterial_RequiereAutenticacion(string metodo, string ruta)
    {
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddSingleton(Mock.Of<IProduccionMaterialRepository>());
        builder.Services.AddSingleton(Mock.Of<IBitacoraService>());
        builder.Services.AddSingleton(Mock.Of<IUsuarioRepository>());
        builder.Services.AddSingleton(Mock.Of<IReporteService>());
        var app = builder.Build();
        new ProduccionMaterialEndpoints().RegistrarEndpoints(app);

        Assert.True(RequiereAutenticacion(EndpointDe(EndpointsDe(app), metodo, ruta)));
    }

    [Theory]
    [InlineData("GET", "/api/recepcion/negocio/{idEntidad:int}")]
    [InlineData("POST", "/api/recepcion/")]
    [InlineData("POST", "/api/recepcion/{idRecepcionDocumento:int}/enviar")]
    public void GrupoRecepcion_RequiereAutenticacion(string metodo, string ruta)
    {
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddSingleton(Mock.Of<IRecepcionRepository>());
        builder.Services.AddSingleton(Mock.Of<IBitacoraService>());
        builder.Services.AddSingleton(Mock.Of<IMensajeriaService>());
        builder.Services.AddSingleton(Mock.Of<IRecepcionEnvioService>());
        builder.Services.AddSingleton(Mock.Of<IRecepcionReporteService>());
        var app = builder.Build();
        new RecepcionEndpoints().RegistrarEndpoints(app);

        Assert.True(RequiereAutenticacion(EndpointDe(EndpointsDe(app), metodo, ruta)));
    }

    private static WebApplication AppDeAuth()
    {
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddSingleton(Mock.Of<IAutenticacionService>());
        builder.Services.AddSingleton(Mock.Of<IUsuarioRepository>());
        builder.Services.AddSingleton(Mock.Of<IBitacoraService>());
        var app = builder.Build();
        new AuthEndpoints().RegistrarEndpoints(app);
        return app;
    }

    [Theory]
    [InlineData("POST", "/api/auth/login")]
    [InlineData("POST", "/api/auth/refresh-token")]
    [InlineData("POST", "/api/auth/olvide-password")]
    [InlineData("PUT", "/api/auth/restablecer-password")]
    public void AuthEndpoints_LoginYRecuperacionDeContrasena_SonPublicos(string metodo, string ruta)
    {
        var endpoints = EndpointsDe(AppDeAuth());
        Assert.False(RequiereAutenticacion(EndpointDe(endpoints, metodo, ruta)));
    }

    [Theory]
    [InlineData("GET", "/api/auth/me")]
    [InlineData("POST", "/api/auth/logout")]
    [InlineData("PUT", "/api/auth/cambiar-password")]
    public void AuthEndpoints_MeLogoutYCambiarPassword_RequierenAutenticacion(string metodo, string ruta)
    {
        var endpoints = EndpointsDe(AppDeAuth());
        Assert.True(RequiereAutenticacion(EndpointDe(endpoints, metodo, ruta)));
    }
}
