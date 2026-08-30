using Karin.AuthService.Client.Interface;
using Microsoft.Extensions.Configuration;
using SilexCore.Domain.Dtos;
using SilexCore.Domain.Interfaces.Infraestructura.Services;

namespace SilexCore.Infrastructure.Services;

public class AutenticacionService(IAuthServiceClient authServiceClient, IConfiguration configuration) : IAutenticacionService
{
    public async Task<(bool Exito, string Mensaje, AuthTokenResponse? Tokens)> LoginAsync(LoginRequest request)
    {
        var response = await authServiceClient.LoginAsync(new Karin.AuthService.Client.Dtos.LoginRequest(request.Email, request.Password));
        if (response.IsSuccessStatusCode && response.Content is not null)
        {
            var c = response.Content;
            return (true, "Inicio de sesión exitoso.", new AuthTokenResponse(c.TokenType, c.AccessToken, c.ExpiresIn, c.RefreshToken));
        }

        return (false, "Usuario o contraseña incorrectos.", null);
    }

    public async Task<(bool Exito, string Mensaje, AuthTokenResponse? Tokens)> RefrescarTokenAsync(RefreshTokenRequest request)
    {
        var response = await authServiceClient.RefreshAsync(new Karin.AuthService.Client.Dtos.RefreshRequest(request.Email, request.RefreshToken));
        if (response.IsSuccessStatusCode && response.Content is not null)
        {
            var c = response.Content;
            return (true, "Token renovado.", new AuthTokenResponse(c.TokenType, c.AccessToken, c.ExpiresIn, c.RefreshToken));
        }

        return (false, "No se pudo renovar el token.", null);
    }

    public async Task<(bool Exito, string Mensaje, string? AuthUserId)> RegistrarIdentidadAsync(RegistrarUsuarioRequest usuario)
    {
        var urlPlataforma = configuration["Settings:UrlPlataforma"] ?? string.Empty;
        var servicio = configuration["Settings:Servicio"] ?? "SilexCore";
        var descripcion = configuration["Settings:Descripcion"] ?? string.Empty;

        var response = await authServiceClient.RegisterAsync(new Karin.AuthService.Client.Dtos.RegisterRequest(
            usuario.NombrePersona, usuario.Email, usuario.Rol, urlPlataforma, servicio, descripcion));

        if (response.IsSuccessStatusCode && response.Content is not null)
            return (true, "Identidad creada correctamente.", response.Content.Mensaje);

        return (false, response.Error?.Message ?? "No se pudo registrar el usuario en el servicio de autenticación.", null);
    }

    public async Task<(bool Exito, string Mensaje)> CambiarPasswordAsync(CambiarPasswordRequest request)
    {
        var response = await authServiceClient.ChangePasswordAsync(new Karin.AuthService.Client.Dtos.ChangePasswordRequest(request.CurrentPassword, request.NewPassword));
        if (response.IsSuccessStatusCode && response.Content is not null)
            return (true, response.Content.Mensaje);

        return (false, response.Error?.Message ?? "No se pudo cambiar la contraseña.");
    }

    public async Task<(bool Exito, string Mensaje)> SolicitarRecuperacionPasswordAsync(OlvidePasswordRequest request)
    {
        var urlPlataforma = configuration["Settings:UrlPlataforma"] ?? string.Empty;
        var servicio = configuration["Settings:Servicio"] ?? "SilexCore";
        var descripcion = configuration["Settings:Descripcion"] ?? string.Empty;

        var response = await authServiceClient.ForgotPasswordAsync(new Karin.AuthService.Client.Dtos.ForgotPasswordRequest(
            request.Email, urlPlataforma, servicio, descripcion));

        if (response.IsSuccessStatusCode && response.Content is not null)
            return (true, response.Content.Mensaje);

        return (false, "No se pudo procesar la solicitud de recuperación.");
    }

    public async Task<(bool Exito, string Mensaje)> RestablecerPasswordAsync(RestablecerPasswordRequest request)
    {
        var response = await authServiceClient.ResetPasswordAsync(new Karin.AuthService.Client.Dtos.ResetPasswordRequest(request.Email, request.ResetToken, request.NewPassword));
        if (response.IsSuccessStatusCode && response.Content is not null)
            return (true, response.Content.Mensaje);

        return (false, response.Error?.Message ?? "No se pudo restablecer la contraseña.");
    }

    public async Task<bool> CerrarSesionAsync()
    {
        var response = await authServiceClient.LogoutAsync(new Karin.AuthService.Client.Dtos.LogoutRequest(string.Empty));
        return response.IsSuccessStatusCode;
    }
}
