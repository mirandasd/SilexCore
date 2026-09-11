namespace SilexCore.Domain.Dtos;

// Estado de vencimiento del certificado de firma de un negocio -- InvoicingService
// es quien tiene acceso al archivo .p12 real y lee la fecha de vencimiento de ahí.
public record EstadoCertificadoResponse
{
    public int IdEntidad { get; init; }
    public string? NombreComercial { get; init; }
    public bool TieneCertificadoRegistrado { get; init; }
    public bool ArchivoEncontrado { get; init; }
    public DateTime? FechaVencimiento { get; init; }
    public int? DiasRestantes { get; init; }
    public bool Vencido { get; init; }
    public string? Mensaje { get; init; }
}

public record GuardarCertificadoResponse
{
    public int IdEntidad { get; init; }
    public string? NombreArchivo { get; init; }
    public DateTime FechaVencimiento { get; init; }
    public string Mensaje { get; init; } = string.Empty;
}
