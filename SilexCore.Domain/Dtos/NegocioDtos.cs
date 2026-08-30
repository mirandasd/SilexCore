namespace SilexCore.Domain.Dtos;

public record NegocioResponse
{
    public int IdEntidad { get; init; }
    public string? RazonSocial { get; init; }
    public string? Identificacion { get; init; }
}

// Datos del negocio para el encabezado/pie de la representación gráfica (Factura/Recepción/Nota).
public record EntidadReporteResponse
{
    public int IdEntidad { get; init; }
    public string? NombreComercial { get; init; }
    public string? RazonSocial { get; init; }
    public string? Identificacion { get; init; }
    public string? TipoIdentificacion { get; init; }
    public string? CodigoPais { get; init; }
    public string? Telefono { get; init; }
    public string? Correo { get; init; }
    public string? Provincia { get; init; }
    public string? Canton { get; init; }
    public string? Distrito { get; init; }
    public string? Direccion { get; init; }
    public string? LogoArchivo { get; init; }
    public string? FondoArchivo { get; init; }
    public string? CuentaLeyenda { get; init; }
    public string? CuentaRepresentante { get; init; }
    public string? CuentaCorriente { get; init; }
    public string? CuentaCliente { get; init; }
    public string? CuentaIban { get; init; }
}

// Leyendas de la representación gráfica (versión de comprobante, texto de
// autorización del decreto) que Hacienda actualiza de vez en cuando --
// vive en una tabla en vez de quemarse en las plantillas HTML.
public record ConfiguracionDocumentoResponse
{
    public string? VersionComprobante { get; init; }
    public string? TextoAutorizacion { get; init; }
}
