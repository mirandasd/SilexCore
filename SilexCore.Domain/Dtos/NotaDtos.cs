namespace SilexCore.Domain.Dtos;

// Registra una nota de crédito sobre una factura existente: toma el consecutivo
// vigente de notas de crédito para el negocio, crea la nota y avanza el
// consecutivo -- todo en una sola transacción.
public record RegistrarNotaCreditoRequest
{
    public int IdEntidad { get; init; }
    public int IdFactura { get; init; }
    public int IdAccion { get; init; }
    public string? Detalle { get; init; }
}

public record RegistrarNotaCreditoResult
{
    public int IdNota { get; init; }
    public string? Consecutivo { get; init; }
    public string? Mensaje { get; init; }
}

// Factura de referencia para armar una nota de crédito -- lo que necesita
// InvoiceReferenceAutocomplete en silex-app para mostrar el resultado de la
// búsqueda por consecutivo.
public record FacturaReferenciaResponse
{
    public int Id { get; init; }
    public string? Consecutivo { get; init; }
    public string? Clave { get; init; }
    public string? Cliente { get; init; }
    public string? Fecha { get; init; }
    public decimal Monto { get; init; }
    public string? Estado { get; init; }
}

// Datos de una nota para su representación gráfica (PDF). tbl_nota no trae
// cliente/receptor/condición-venta/moneda/detalle-de-líneas propios -- se
// heredan de la factura referenciada (IdFactura), mismo criterio que usa
// SP_ObtenerNotaXml para armar el XML real que se manda a Hacienda.
public record NotaResponse
{
    public int IdFactura { get; init; }
    public int IdEntidad { get; init; }
    public int IdCliente { get; init; }
    public string? Consecutivo { get; init; }
    public string? Clave { get; init; }
    public string? Fecha { get; init; }
    public string? Accion { get; init; }
    public string? Razon { get; init; }
    public string? FacturaConsecutivo { get; init; }
    public string? FacturaClave { get; init; }
    public string? CondicionVenta { get; init; }
    public string? PlazoCredito { get; init; }
    public string? MedioPago { get; init; }
    public string? Moneda { get; init; }
    public decimal MonedaValor { get; init; }
}
