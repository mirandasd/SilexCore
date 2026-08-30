using System.Security.Claims;
using SilexCore.Api.Interfaces;
using SilexCore.Domain.Dtos;
using SilexCore.Domain.Interfaces.Infraestructura.Persistence;
using SilexCore.Domain.Interfaces.Infraestructura.Services;

namespace SilexCore.Api.Endpoints.v1;

public class AuthEndpoints : IEndpointModule
{
    public void RegistrarEndpoints(IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup("/api/auth")
                       .WithTags("Autenticación");

        // --- LOGIN ---

        // Inicia sesión y enriquece la respuesta con el perfil local si existe
        grupo.MapPost("/login", async (LoginRequest credenciales, IAutenticacionService autenticacion, IUsuarioRepository usuarioRepo, IBitacoraService bitacora) =>
        {
            var (exito, mensaje, tokens) = await autenticacion.LoginAsync(credenciales);
            if (!exito || tokens is null)
            {
                await bitacora.RegistrarErrorBitacoraAsync($"Login fallido para {credenciales.Email}", mensaje);
                return Results.Json(new { mensaje }, statusCode: 401);
            }

            var perfil = await usuarioRepo.ObtenerPerfilPorEmailAsync(credenciales.Email);
            await bitacora.RegistrarNegocioBitacoraAsync($"Login exitoso de {credenciales.Email}", mensaje);

            return Results.Ok(new LoginResponse(tokens, perfil));
        })
        .WithName("Login");

        // Verifica la sesión actual a partir del JWT (no depende de la base de datos de negocio)
        grupo.MapGet("/me", (ClaimsPrincipal usuario) =>
        {
            var id = usuario.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var email = usuario.FindFirst(ClaimTypes.Email)?.Value;
            var roles = usuario.FindAll(ClaimTypes.Role).Select(c => c.Value);
            var vistas = usuario.FindAll("permiso_vista").Select(c => c.Value);
            var debeCambiarPassword = usuario.FindFirst("debe_cambiar_password")?.Value == "true";

            return Results.Ok(new { id, email, roles, vistas, debeCambiarPassword });
        })
        .RequireAuthorization()
        .WithName("Me");

        // Renueva el access token a partir del refresh token
        grupo.MapPost("/refresh-token", async (RefreshTokenRequest request, IAutenticacionService autenticacion) =>
        {
            var (exito, mensaje, tokens) = await autenticacion.RefrescarTokenAsync(request);
            return exito ? Results.Ok(tokens) : Results.Json(new { mensaje }, statusCode: 401);
        })
        .WithName("RefrescarToken");

        // Cierra la sesión del usuario autenticado
        grupo.MapPost("/logout", async (IAutenticacionService autenticacion) =>
        {
            var exito = await autenticacion.CerrarSesionAsync();
            return exito ? Results.Ok(new { mensaje = "Sesión cerrada exitosamente." }) : Results.BadRequest(new { mensaje = "No se pudo cerrar la sesión." });
        })
        .RequireAuthorization()
        .WithName("Logout");

        // --- CONTRASEÑA ---

        // Cambia la contraseña del usuario autenticado (requiere contraseña actual)
        grupo.MapPut("/cambiar-password", async (CambiarPasswordRequest request, IAutenticacionService autenticacion, IBitacoraService bitacora) =>
        {
            var (exito, mensaje) = await autenticacion.CambiarPasswordAsync(request);

            if (!exito)
            {
                await bitacora.RegistrarErrorBitacoraAsync("Error al cambiar contraseña", mensaje);
                return Results.BadRequest(new { mensaje });
            }

            await bitacora.RegistrarNegocioBitacoraAsync("Contraseña actualizada por el usuario", mensaje);
            return Results.Ok(new { mensaje });
        })
        .RequireAuthorization()
        .WithName("CambiarPassword");

        // Genera una nueva contraseña temporal y la envía por correo (usuario no autenticado)
        grupo.MapPost("/olvide-password", async (OlvidePasswordRequest request, IAutenticacionService autenticacion, IBitacoraService bitacora) =>
        {
            var (_, mensaje) = await autenticacion.SolicitarRecuperacionPasswordAsync(request);
            await bitacora.RegistrarNegocioBitacoraAsync($"Solicitud de recuperación de contraseña para {request.Email}", mensaje);
            return Results.Ok(new { mensaje });
        })
        .WithName("OlvidePassword");

        // Restablece la contraseña usando el token generado por /olvide-password
        grupo.MapPut("/restablecer-password", async (RestablecerPasswordRequest request, IAutenticacionService autenticacion, IBitacoraService bitacora) =>
        {
            var (exito, mensaje) = await autenticacion.RestablecerPasswordAsync(request);

            if (!exito)
            {
                await bitacora.RegistrarErrorBitacoraAsync($"Error al restablecer contraseña de {request.Email}", mensaje);
                return Results.BadRequest(new { mensaje });
            }

            await bitacora.RegistrarNegocioBitacoraAsync($"Contraseña restablecida para {request.Email}", mensaje);
            return Results.Ok(new { mensaje });
        })
        .WithName("RestablecerPassword");
    }
}
