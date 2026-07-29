namespace SilexCore.Domain.Dtos;


public record RegistrarFacturaRequest;
public record RegistrarFacturaResult;
public record FacturaBase1
{    
    public string? Consecutivo { get; init; }
    public string? Fecha { get; init; }
    public string? TipoComprobante { get; init; }
    public string? CondicionVenta { get; init; }
    public string? PlazoCredito { get; init; }
    public string? Moneda { get; init; }
    public decimal MonedaValor { get; init; }
    public string? MedioPago { get; init; }
    public string? Detalle { get; init; }
    public ClienteResponse? Cliente { get; init; }
    //public List<DetalleFacturaDto>? DetalleFactura { get; init; }
}

//public record FacturaResponse : FacturaBase1
//{    
//    public int IdFactura{ get; init; }
//    public string? Clave { get; init; }
//}

public class FacturaBase
{
    public string? Consecutivo { get; init; }
    public string? PlazoCredito { get; init; }
    public decimal MonedaValor { get; init; }
    public string? Detalle { get; init; }
}

public class FacturaRequest : FacturaBase
{
    public int IdEntidad { get; init; }
    public int IdCliente { get; init; }
    public int IdTipoComprobante { get; init; } = 1;
    public int IdCondicionVenta { get; init; }
    public int IdMoneda { get; init; }
    public int IdMedioPago { get; init; }
    public decimal PagoCliente { get; init; } = 0;
    public decimal VueltoCliente { get; init; } = 0;
    public string? CodigoActividad { get; init; }
}


public class AgregarFacturaRequest : FacturaRequest
{
    public List<AgregarDetalleFacturaRequest?> Detalle { get; set; }
}

public record FacturaResponse : FacturaBase1
{
    public int IdFactura { get; init; }
    public string? Clave { get; init; }
}

public record FacturaPorConsecutivoResponse
{
    public int IdFactura { get; init; }
    public string? Consecutivo { get; init; }
    public string? Clave { get; init; }
    public string? Fecha { get; init; }
}

public record HistoricoFacturaDto
{
    public string? Consecutivo { get; init; }
    public string? Clave { get; init; }
    public string? Fecha { get; init; }
}

public record DetalleFacturaBase
{
    public int IdFactura { get; init; }
    public int IdExoneracion { get; init; }
    public string? MedidaComercial { get; init; }
    public decimal Cantidad { get; init; }
    public decimal Precio { get; init; }
    public string? Detalle { get; init; }
    public int PorcentajeDescuento { get; init; }
    public string? NaturalezaDescuento { get; init; }
}

public record DetalleFacturaResponse : DetalleFacturaBase
{
    public string? Codigo { get; init; } 
    public string? Opcion { get; init; }
    public string? CodigoProducto { get; init; }
    public string? UnidadMedida { get; init; }
    public string? CodigoCabys { get; init; }
    public string? TipoImpuesto { get; init; }
    public string? TipoTarifa { get; init; }
    public int ValorTarifa { get; init; }

    //public ExoneracionRequest? Exoneracion { get; init; }
}


public record AgregarDetalleFacturaRequest : DetalleFacturaBase
{
    public int IdOpcionVenta { get; init; }
    public int IdCodigoProducto { get; init; }
    public int IdUnidadMedida { get; init; }  
    public int IdTipoImpuesto { get; init; }
    public int IdTipoTarifa { get; init; }

    public ExoneracionRequest? Exoneracion { get; init; }

}

public record ExoneracionBase
{
    public int IdTipoDocumentoExoneracion { get; init; } = 0;
    public string? NumeroDocumento { get; init; }
    public int Articulo { get; init; } = 0;
    public int Inciso { get; init; } = 0;
    public int IdEntidadEmisora { get; init; } = 0;
    public string? FechaEmision { get; init; }
    public int Porcentaje { get; init; } = 0;
    public string? TipoAutorizacion { get; init; }
    public int Autorizacion { get; init; } = 0;
    public int Anno { get; init; } = 0;
    public int CodigoProyectoCFIA { get; init; } = 0;
    
}

public record ExoneracionRequest : ExoneracionBase
{    
    public string? Identificacion { get; init; }
    
}

public record ExoneracionResponse : ExoneracionBase
{
    public int? IdExoneracion { get; init; }
}