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

// Resultado de la consulta EN VIVO a Hacienda (GET /fe/ex) -- no confundir con
// ExoneracionResponse (Dtos/FacturacionDtos.cs), que es el registro YA GUARDADO
// para un cliente. Este es el paso previo: se consulta, se le muestra al usuario,
// y si la aplica, ahí se persiste con IFacturaRepository.AgregarExoneracionAsync.
public record ExoneracionHaciendaResponse
{
    public string? NumeroDocumento { get; set; }
    public string? Identificacion { get; set; }
    public int CodigoProyectoCFIA { get; set; }
    public decimal PorcentajeExoneracion { get; set; }
    public int Autorizacion { get; set; }
    public string? FechaEmision { get; set; }
    public string? FechaVencimiento { get; set; }
    public int Ano { get; set; }
    public List<ExoneracionCabysHaciendaResponse>? Cabys { get; set; }
    public string? TipoAutorizacion { get; set; }
    public TipoDocumentoExoneracionHaciendaResponse? TipoDocumento { get; set; }
    public string? CodigoInstitucion { get; set; }
    public string? NombreInstitucion { get; set; }
    public bool PoseeCabys { get; set; }
}

public record ExoneracionCabysHaciendaResponse
{
    public string? Codigo { get; set; }
    public string? Descripcion { get; set; }
}

public record TipoDocumentoExoneracionHaciendaResponse
{
    public string? Codigo { get; set; }
    public string? Descripcion { get; set; }
}
