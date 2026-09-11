using System.Net;
using Karin.AuthService.Client.Dtos;
using Karin.AuthService.Client.Interface;
using Microsoft.Extensions.Configuration;
using Moq;
using Refit;
using SilexCore.Domain.Dtos;
using SilexCore.Infrastructure.Services;

namespace SilexCore.Tests;

// Cubre el bug corregido el 2026-09-10: RegistrarIdentidadAsync guardaba el
// mensaje de texto de AuthService ("Usuario registrado exitosamente.") como
// AuthUserId en vez del GUID real (GenericResponse.UsuarioId). Eso corrompía
// auth_user_id en tbl_usuario_perfil para cualquier usuario que no fuera el
// primero registrado en el sistema.
public class AutenticacionServiceTests
{
    private static RegistrarUsuarioRequest NuevoUsuarioDePrueba() => new()
    {
        IdEntidad = 1,
        NombrePersona = "Cajero Prueba",
        Email = "cajero.prueba@silexcore.test",
        Rol = "Cajero"
    };

    private static IConfiguration ConfiguracionVacia() => new ConfigurationBuilder().Build();

    private static HttpResponseMessage RespuestaHttp(HttpStatusCode codigo) => new(codigo)
    {
        RequestMessage = new HttpRequestMessage(HttpMethod.Post, "http://localhost/api/internal/auth/register")
    };

    private static ApiResponse<GenericResponse> RespuestaExitosa(GenericResponse contenido) =>
        new(RespuestaHttp(HttpStatusCode.OK), contenido, new RefitSettings());

    private static ApiResponse<GenericResponse> RespuestaFallida(HttpStatusCode codigo) =>
        new(RespuestaHttp(codigo), null, new RefitSettings());

    [Fact]
    public async Task RegistrarIdentidadAsync_CuandoAuthServiceResponde200_DevuelveElGuidComoAuthUserId()
    {
        var guidReal = Guid.NewGuid().ToString();
        var mockClient = new Mock<IAuthServiceClient>();
        mockClient
            .Setup(c => c.RegisterAsync(It.IsAny<RegisterRequest>()))
            .ReturnsAsync(RespuestaExitosa(new GenericResponse("Usuario registrado exitosamente.", null, guidReal)));

        var service = new AutenticacionService(mockClient.Object, ConfiguracionVacia());

        var (exito, _, authUserId) = await service.RegistrarIdentidadAsync(NuevoUsuarioDePrueba());

        Assert.True(exito);
        Assert.Equal(guidReal, authUserId);
        // Regresión explícita del bug: el mensaje de texto NUNCA debe terminar
        // en el campo AuthUserId.
        Assert.NotEqual("Usuario registrado exitosamente.", authUserId);
    }

    [Fact]
    public async Task RegistrarIdentidadAsync_CuandoAuthServiceFalla_NoDevuelveIdentidadNiExito()
    {
        var mockClient = new Mock<IAuthServiceClient>();
        mockClient
            .Setup(c => c.RegisterAsync(It.IsAny<RegisterRequest>()))
            .ReturnsAsync(RespuestaFallida(HttpStatusCode.BadRequest));

        var service = new AutenticacionService(mockClient.Object, ConfiguracionVacia());

        var (exito, _, authUserId) = await service.RegistrarIdentidadAsync(NuevoUsuarioDePrueba());

        Assert.False(exito);
        Assert.Null(authUserId);
    }
}
