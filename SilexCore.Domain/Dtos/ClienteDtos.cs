namespace SilexCore.Domain.Dtos;

// 1. Modelo Base para Clientes Completo
public record Cliente
{
    public int IdTipoIdentificacion { get; init; }
    public string? Identificacion { get; init; }
    public string? Nombre { get; init; }
    public string? CodigoPais { get; init; }
    public string? Telefono { get; init; }
    public int IdProvincia { get; init; } = 0;
    public int IdCanton { get; init; } = 0;
    public int IdDistrito { get; init; } = 0;
    public string? Direccion { get; init; }
}

public record CorreoElectronico
{
    public string? Correo { get; init; }
}

public record Regimen
{
    public int Codigo { get; init; }
    public string? Descripcion { get; init; }
}

public record Actividad
{
    public string? Tipo { get; init; }
    public string? Codigo { get; init; }
    public string? Descripcion { get; init; }
}

public record ClienteSimpleResponse
{
    public int IdCliente { get; init; } = 0;
    public string? Nombre { get; init; }
}

public record ClienteResponse : Cliente
{
    public int IdCliente { get; init; } = 0;
    public string? Estado { get; init; }

    public List<CorreoElectronico>? Correos { get; init; }
    public Regimen? Regimen { get; init; }
    public List<Actividad>? Actividades { get; init; }
}

public record NewClienteRequest : Cliente;

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
public record NewClienteActividadRequest : Actividad
{
    public int IdCliente { get; init; } = 0;
}

public record NewClienteRegimenRequest : Regimen
{
    public int IdCliente { get; init; } = 0;
}