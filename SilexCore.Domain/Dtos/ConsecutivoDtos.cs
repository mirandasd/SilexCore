namespace SilexCore.Domain.Dtos;

public record ConsecutivoResponse
{
    public int IdConsecutivo { get; init; }
    public string? Tipo { get; init; }
    public string? PuntoDeVenta { get; init; }
    public string? Sucursal { get; init; }
    public string? DocumentoAsociado { get; init; }
    public string? Consecutivo { get; init; }
}
