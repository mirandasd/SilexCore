namespace SilexCore.Domain.Dtos;

public record CategoriaVentaResponse
{
    public int IdCategoria { get; init; }
    public string? CodCategoria { get; init; }
    public string? Descripcion { get; init; }
    public bool Estado { get; init; }
}

public record OpcionVentaResponse
{
    public int IdOpcionVenta { get; init; }
    public string? Codigo { get; init; }
    public string? Tipo { get; init; }
    public string? UnidadMedida { get; init; }
    public int Categoria { get; init; }
    public string? Opcion { get; init; }
    public string? CodigoCabys { get; init; }
    public string? DetalleCabys { get; init; }
}
