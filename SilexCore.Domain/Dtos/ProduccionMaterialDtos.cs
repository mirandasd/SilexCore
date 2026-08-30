namespace SilexCore.Domain.Dtos;

public record ProduccionMaterialRequest
{
    public int IdEntidad { get; init; }
    public DateTime Fecha { get; init; }
    public decimal PiedraCuarta { get; init; }
    public decimal PolvoPiedra { get; init; }
    public decimal Arena { get; init; }
    public decimal Lastre { get; init; }
}

public record ActualizarProduccionMaterialRequest : ProduccionMaterialRequest
{
    public int IdProduccion { get; init; }
}

public record ProduccionMaterialResponse
{
    public int IdProduccion { get; init; }
    public int IdEntidad { get; init; }
    public DateTime Fecha { get; init; }
    public decimal PiedraCuarta { get; init; }
    public decimal PolvoPiedra { get; init; }
    public decimal Arena { get; init; }
    public decimal Lastre { get; init; }
}

public record AgregarProduccionMaterialResponse
{
    public int IdProduccion { get; init; }
    public string? Mensaje { get; init; }
}

// Balance de apilado: producido - vendido (facturado) por material, en un rango de fechas.
public record BalanceApiladoResponse
{
    public string? Material { get; init; }
    public decimal TotalProducido { get; init; }
    public decimal TotalVendido { get; init; }
    public decimal Balance { get; init; }
}
