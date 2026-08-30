using SilexCore.Domain.Dtos;

namespace SilexCore.Domain.Interfaces.Infraestructura.Services;

public interface IAutenticacionService
{
    Task<(bool Exito, string Mensaje, AuthTokenResponse? Tokens)> LoginAsync(LoginRequest request);
    Task<(bool Exito, string Mensaje, AuthTokenResponse? Tokens)> RefrescarTokenAsync(RefreshTokenRequest request);
    Task<(bool Exito, string Mensaje, string? AuthUserId)> RegistrarIdentidadAsync(RegistrarUsuarioRequest usuario);
    Task<(bool Exito, string Mensaje)> CambiarPasswordAsync(CambiarPasswordRequest request);
    // AuthService genera una contraseña temporal nueva, la aplica y la envía por
    // correo directamente -- el usuario no elige su propia contraseña acá.
    Task<(bool Exito, string Mensaje)> SolicitarRecuperacionPasswordAsync(OlvidePasswordRequest request);
    Task<(bool Exito, string Mensaje)> RestablecerPasswordAsync(RestablecerPasswordRequest request);
    Task<bool> CerrarSesionAsync();
}
