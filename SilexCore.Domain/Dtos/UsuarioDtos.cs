namespace SilexCore.Domain.Dtos;

public record RegistrarUsuarioRequest
{
    public int IdEntidad { get; init; }
    public string NombrePersona { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Rol { get; init; } = string.Empty;
    public string? Telefono { get; init; }
    public string? Cargo { get; init; }
}

public record ActualizarPerfilRequest
{
    public int IdUsuario { get; init; }
    public string Nombre { get; init; } = string.Empty;
    public string? Telefono { get; init; }
    public string? Cargo { get; init; }
    public string? FotoUrl { get; init; }
}

public record PerfilUsuarioResponse
{
    public int IdUsuario { get; init; }
    public int IdEntidad { get; init; }
    public string? AuthUserId { get; init; }
    public string Nombre { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string? Telefono { get; init; }
    public string? Cargo { get; init; }
    public string? FotoUrl { get; init; }
    public string Rol { get; init; } = string.Empty;
    public bool Activo { get; init; }
}
