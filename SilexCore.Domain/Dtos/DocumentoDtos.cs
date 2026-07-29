namespace SilexCore.Domain.Dtos;

public record DocumentosPorNegocioResponse 
{
    public int Id { get; init; }
    public string? NombreCliente { get; init; }
    public string? Fecha { get; init; }
    public string? Consecutivo { get; init; }
    public int EstadoEnvio { get; init; }
    public int EstadoHacienda { get; init; }

    public string? Enviado { get; init; }
    public string? Validacion { get; init; }
    public string? DocumentoPdf { get; init; }
    public string? DocumentoRespuesta { get; init; }
}



