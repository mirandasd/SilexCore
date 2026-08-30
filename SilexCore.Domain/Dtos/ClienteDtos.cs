namespace SilexCore.Domain.Dtos;

// 1. Modelo Base para Clientes Completo
public record Cliente
{
    public int IdTipoIdentificacion { get; init; }
    public string? Identificacion { get; init; }
    public string? Nombre { get; init; }
    public string? Correo { get; init; }
    public string? CodigoPais { get; init; }
    public string? Telefono { get; init; }
    public int IdProvincia { get; init; } = 0;
    public int IdCanton { get; init; } = 0;
    public int IdDistrito { get; init; } = 0;
    public string? Direccion { get; init; }
}

public record Regimen
{
    public int Codigo { get; init; }
    public string? Descripcion { get; init; }
}

// clienteactividad.estado es tinyint(1) (activo/inactivo de la relación), no el
// estado de la actividad en Hacienda -- no confundir con datos de la ATV.
public record Actividad
{
    public bool Estado { get; init; }
    public string? Tipo { get; init; }
    public string? Codigo { get; init; }
    public string? Descripcion { get; init; }
}

// Coincide con las columnas planas que devuelven ObtenerClientePorId /
// ObtenerClientes / ObtenerClientePorNombreOIdentificacion en sw_cliente.
// Regimen/Actividades se completan aparte (ObtenerClienteRegimen /
// ObtenerClienteActividades) solo cuando se pide el detalle de un cliente.
public record ClienteResponse : Cliente
{
    public int IdCliente { get; init; } = 0;
    public bool Estado { get; init; }

    public Regimen? Regimen { get; init; }
    public List<Actividad>? Actividades { get; init; }
}

public record NewClienteRequest : Cliente;

public record AgregarClienteResponse
{
    public int IdCliente { get; init; }
    public string? Mensaje { get; init; }
}

public record UpdateClienteRequest : Cliente
{
    public int IdCliente { get; init; } = 0;
}

// 2. Modelo Base para Clientes Simplificado / Mínimo
public record ClienteMinimo
{
    public int IdTipoIdentificacion { get; init; }
    public string? Identificacion { get; init; }
    public string? Nombre { get; init; }
    public string? Correo { get; init; }
}

public record NewClienteMinimoRequest : ClienteMinimo
{
    public List<NewClienteActividadRequest>? Actividades { get; init; }
    public NewClienteRegimenRequest? Regimen { get; init; }
}

// 3. Sub-modelos relacionados
// No hereda de Actividad: AgregarClienteActividad (SP) no recibe "estado",
// ese flag lo controla la propia relación clienteactividad por su cuenta.
public record NewClienteActividadRequest
{
    public int IdCliente { get; init; } = 0;
    public string? Tipo { get; init; }
    public string? Codigo { get; init; }
    public string? Descripcion { get; init; }
}

public record NewClienteRegimenRequest : Regimen
{
    public int IdCliente { get; init; } = 0;
}