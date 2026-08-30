namespace SilexCore.Domain.Dtos;

// Registro completo de una factura: resuelve/crea el cliente si hace falta,
// toma el consecutivo vigente, crea la factura, recorre el detalle (con o sin
// exoneración por línea) y al final actualiza el consecutivo. Todo en una sola
// transacción -- si cualquier paso falla, no debe quedar nada a medias.
public record RegistrarFacturaRequest
{
    public int IdEntidad { get; init; }
    public int IdTipoComprobante { get; init; } = 1;
    public string? CodigoActividad { get; init; }
    public int IdCondicionVenta { get; init; }
    public string? PlazoCredito { get; init; }
    public int IdMoneda { get; init; }
    public decimal MonedaValor { get; init; }
    public int IdMedioPago { get; init; }
    public decimal PagoCliente { get; init; }
    public decimal VueltoCliente { get; init; }
    public string? Detalle { get; init; }

    public ClienteFacturaRequest Cliente { get; init; } = null!;
    public List<AgregarDetalleFacturaRequest> Lineas { get; init; } = new();
}

// Si IdCliente > 0, ya existe en sw_cliente y se usa directo. Si es 0, se
// crea/completa vía AgregarClienteMinimoExtendido -- el caso típico de un
// cliente recién encontrado en Hacienda, que todavía no tiene id local.
public record ClienteFacturaRequest
{
    public int IdCliente { get; init; } = 0;
    public int IdTipoIdentificacion { get; init; }
    public string? Identificacion { get; init; }
    public string? Nombre { get; init; }
    public string? Correo { get; init; }
    public string? RegimenDescripcion { get; init; }
    public int RegimenCodigo { get; init; }
    public string? ActividadDescripcion { get; init; }
    public string? ActividadTipo { get; init; }
    public string? ActividadCodigo { get; init; }
}

public record RegistrarFacturaResult
{
    public int IdFactura { get; init; }
    public int IdCliente { get; init; }
    public string? Consecutivo { get; init; }
    public string? Mensaje { get; init; }
}

// Resultado de enviar una factura ya registrada a Hacienda (vía InvoicingService).
public record EnviarFacturaResult
{
    public string? Clave { get; init; }
    public string? Consecutivo { get; init; }
    public string? Estado { get; init; }
    public string? Mensaje { get; init; }
}

public record FacturaBase1
{
    public int IdEntidad { get; init; }
    public int IdCliente { get; init; }
    public string? Consecutivo { get; init; }
    public string? Fecha { get; init; }
    public string? TipoComprobante { get; init; }
    public string? CondicionVenta { get; init; }
    public string? PlazoCredito { get; init; }
    public string? Moneda { get; init; }
    public decimal MonedaValor { get; init; }
    public string? MedioPago { get; init; }
    public decimal PagoCliente { get; init; }
    public decimal VueltoCliente { get; init; }
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

    // CABYS efectivamente usado en esta línea -- normalmente el de la opción de
    // venta (tbl_opcion_venta.codigoCabys), pero el usuario lo puede cambiar; se
    // guarda tal cual quedó para que InvoicingService no tenga que adivinarlo.
    public string? CodigoCabys { get; init; }

    public ExoneracionRequest? Exoneracion { get; init; }

}

public record ExoneracionBase
{
    public string? NumeroDocumento { get; init; }
    public int Articulo { get; init; } = 0;
    public int Inciso { get; init; } = 0;
    public string? FechaEmision { get; init; }
    public string? FechaVencimiento { get; init; }
    public int Porcentaje { get; init; } = 0;
    public string? TipoAutorizacion { get; init; }
    public int Autorizacion { get; init; } = 0;
    public int Anno { get; init; } = 0;
    public int CodigoProyectoCFIA { get; init; } = 0;
    public bool PoseeCabys { get; init; } = false;
}

// Lo que se necesita para registrar (o recargar, vía dedup en el SP) una
// exoneración para un cliente. CodigoInstitucion/TipoDocumentoCodigo son los
// códigos de catálogo de Hacienda (ej. "01", "04") -- el SP resuelve los ids
// internos. Cabys solo aplica cuando PoseeCabys es true.
public record ExoneracionRequest : ExoneracionBase
{
    public string? Identificacion { get; init; }
    public string? CodigoInstitucion { get; init; }
    public string? TipoDocumentoCodigo { get; init; }
    public List<ExoneracionCabysItem>? Cabys { get; init; }
}

public record ExoneracionCabysItem
{
    public string? CodigoCabys { get; init; }
    public string? DetalleCabys { get; init; }
}

public record ExoneracionResponse : ExoneracionBase
{
    public int? IdExoneracion { get; init; }
    public string? NombreInstitucion { get; init; }
    public string? TipoDocumentoCodigo { get; init; }
    public string? TipoDocumentoDescripcion { get; init; }
}