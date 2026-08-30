namespace SilexCore.Domain.Dtos;

public record RecepcionRequest
{
    public int IdEntidad { get; init; }
    public string? NombreEmisor { get; init; }
    public string? TipoIdentificacion { get; init; }
    public string? Identificacion { get; init; }
    public int IdTipocomprobante { get; init; }
    public string? Clave { get; init; }
    public decimal Impuesto { get; init; }
    public decimal ImpuestoAcreditar { get; init; }
    public decimal GastoAplicable { get; init; }
    public decimal Total { get; init; }
    public int IdMoneda { get; init; }
    public decimal MonedaValor { get; init; }
    public string? Detalle { get; init; }
    public int IdEstadoRecepcion { get; init; }
    public int IdCondicionImpuesto { get; init; }
}

// Detalle completo de una recepción (para la representación gráfica / reporte).
public record RecepcionDetalleResponse
{
    public int IdRecepcionDocumento { get; init; }
    public int IdEntidad { get; init; }
    public int IdCliente { get; init; }
    public string? NombreEmisor { get; init; }
    public string? IdentificacionEmisor { get; init; }
    public string? TipoIdentificacionEmisor { get; init; }
    public string? TelefonoEmisor { get; init; }
    public string? CorreoEmisor { get; init; }
    public string? TipoComprobante { get; init; }
    public string? Clave { get; init; }
    public string? Consecutivo { get; init; }
    public string? Fecha { get; init; }
    public string? EstadoRecepcion { get; init; }
    public decimal Impuesto { get; init; }
    public decimal ImpuestoAcreditar { get; init; }
    public decimal GastoAplicable { get; init; }
    public decimal Total { get; init; }
    public string? Moneda { get; init; }
    public decimal MonedaValor { get; init; }
    public string? Detalle { get; init; }
    public string? CondicionImpuesto { get; init; }
}
