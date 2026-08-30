namespace SilexCore.Domain.Dtos;

public record LoginRequest(string Email, string Password);
public record RefreshTokenRequest(string Email, string RefreshToken);
public record OlvidePasswordRequest(string Email);
public record RestablecerPasswordRequest(string Email, string ResetToken, string NewPassword);
public record CambiarPasswordRequest(string CurrentPassword, string NewPassword);

public record AuthTokenResponse(string TokenType, string AccessToken, int ExpiresIn, string RefreshToken);
public record LoginResponse(AuthTokenResponse Tokens, PerfilUsuarioResponse? Perfil);
