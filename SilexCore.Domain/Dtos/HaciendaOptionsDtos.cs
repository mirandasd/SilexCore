namespace SilexCore.Domain.Dtos;

public record CabysResponse
{
    public string? Codigo { get; set; }
    public string? Descripcion { get; set; }
    public int Impuesto { get; set; }
}

public class DolarResponse
{
    public VentaResponse? Venta { get; set; }
}

public class VentaResponse
{
    public string? Fecha { get; set; }
    public decimal Valor { get; set; }
}


public class ContribuyenteResponse
{
    public string? Nombre { get; set; }
    public string? TipoIdentificacion { get; set; }
    public string? Identificacion { get; set; }
    public string? Correo { get; set; }
}

public record OpcionDeFacturacionResponse
{
    public int Id { get; set; }
    public string? Descripcion { get; set; }
}

public record OpcionDeFacturacionConValorResponse : OpcionDeFacturacionResponse
{
    public decimal Valor { get; set; }
}

public record CantonResponse
{
    public int IdCanton { get; set; }
    public string? Canton { get; set; }
}

public record DistritoResponse
{
    public int IdDistrito { get; set; }
    public string? Distrito { get; set; }
}

public record ProvinciaResponse
{
    public int IdProvincia { get; set; }
    public string? Provincia { get; set; }
}
