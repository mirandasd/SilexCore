using Karin.Persistence.Dapper.Interfaces;
using SilexCore.Domain.Constants;
using SilexCore.Domain.Dtos;
using SilexCore.Domain.Interfaces.Infraestructura.Persistence;

namespace SilexCore.Infrastructure.Persistence;

public class ClienteRepository(ISqlExecutor sqlExecutor) : IClienteRepository
{
    #region private methods
    private Task<IEnumerable<T>> QueryAsync<T>(string spName, object? parameters = null)
        => sqlExecutor.QueryAsync<T>(spName, parameters, connectionName: ConnectionNames.Cliente);

    private Task<T?> QueryFirstOrDefaultAsync<T>(string spName, object? parameters = null)
        => sqlExecutor.QueryFirstOrDefaultAsync<T>(spName, parameters, connectionName: ConnectionNames.Cliente);

    private Task<(int RowsAffected, string OutputMessage)> ExecSpAsync(string spName, object? parameters = null)
        => sqlExecutor.ExecuteSpWithOutputAsync(spName, parameters, outputParamName: "msj", connectionName: ConnectionNames.Cliente);

    #endregion private methods

    public Task<(int RowsAffected, string Mensaje)> ActivarClienteAsync(int idCliente)
        => ExecSpAsync("ActivarCliente", new { IdCliente = idCliente });

    public Task<(int RowsAffected, string Mensaje)> ActualizarClienteAsync(UpdateClienteRequest cliente)
        => ExecSpAsync("ActualizarCliente", cliente);

    // Requiere Base de datos/sw_cliente_actualizaciones.sql aplicado (agrega OUT msj
    // a AgregarClienteRegimen/AgregarClienteActividad, que en producción no lo tenían).
    public Task<(int RowsAffected, string Mensaje)> AgregarClienteRegimenAsync(NewClienteRegimenRequest regimen)
        => ExecSpAsync("AgregarClienteRegimen", regimen);

    public Task<(int RowsAffected, string Mensaje)> AgregarClienteActividadAsync(NewClienteActividadRequest actividad)
        => ExecSpAsync("AgregarClienteActividad", actividad);

    public Task<(int RowsAffected, string Mensaje)> EliminarClienteAsync(int idCliente)
        => ExecSpAsync("EliminarCliente", new { IdCliente = idCliente });

    public async Task<ClienteResponse?> ObtenerClientePorIdAsync(int idCliente)
    {
        var cliente = await QueryFirstOrDefaultAsync<ClienteResponse>("ObtenerClientePorId", new { idCliente });
        if (cliente is null)
            return null;

        var regimenes = await QueryAsync<Regimen>("ObtenerClienteRegimen", new { idCliente });
        var actividades = await QueryAsync<Actividad>("ObtenerClienteActividades", new { idCliente });

        return cliente with
        {
            Regimen = regimenes.FirstOrDefault(),
            Actividades = actividades.ToList()
        };
    }

    public Task<IEnumerable<ClienteResponse>> ObtenerClientePorNombreOIdentificacionAsync(string busqueda)
        => QueryAsync<ClienteResponse>("ObtenerClientePorNombreOIdentificacion", new { dato = busqueda });

    // ObtenerClientes() no acepta parámetros en la BD actual (sin paginado todavía).
    public Task<IEnumerable<ClienteResponse>> ObtenerClientesAsync()
        => QueryAsync<ClienteResponse>("ObtenerClientes");

    public Task<int> ObtenerTotalClientesAsync()
        => QueryFirstOrDefaultAsync<int>("ObtenerTotalClientes");

    public async Task<AgregarClienteResponse> AgregarClienteAsync(NewClienteRequest cliente)
        => await QueryFirstOrDefaultAsync<AgregarClienteResponse>("AgregarCliente", cliente)
           ?? new AgregarClienteResponse { IdCliente = 0, Mensaje = "No se pudo agregar el cliente." };
}
