using Dapper;
using Karin.Persistence.Dapper.Interfaces;
using SilexCore.Domain.Constants;
using SilexCore.Domain.Dtos;
using SilexCore.Domain.Interfaces.Infraestructura.Persistence;
using SilexCore.Infrastructure.Persistence.Extensions;
using System.Data;

namespace SilexCore.Infrastructure.Persistence;

public class ClienteRepository(ISqlExecutor sqlExecutor) : IClienteRepository
{
    #region private methods
    private Task<IEnumerable<T>> QueryAsync<T>(string spName, object? parameters = null)
        => sqlExecutor.QueryAsync<T>(spName, parameters, connectionName: ConnectionNames.Cliente);

    private Task<T?> QueryFirstOrDefaultAsync<T>(string spName, object? parameters = null)
        => sqlExecutor.QueryFirstOrDefaultAsync<T>(spName, parameters, connectionName: ConnectionNames.Cliente);

    private Task<SqlMapper.GridReader> QueryMultipleAsync(string spName, object? parameters = null)
        => sqlExecutor.QueryMultipleAsync(spName, parameters, connectionName: ConnectionNames.Cliente);

    private Task<(int RowsAffected, string OutputMessage)> ExecSpAsync(string spName, object? parameters = null)
        => sqlExecutor.ExecuteSpWithOutputAsync(spName, parameters, outputParamName: "msj", connectionName: ConnectionNames.Cliente);

    #endregion private methods

    public Task<(int RowsAffected, string Mensaje)> ActivarClienteAsync(int idCliente)
        => ExecSpAsync("ActivarCliente", new { idCliente });

    public Task<(int RowsAffected, string Mensaje)> ActualizarClienteAsync(UpdateClienteRequest cliente)
        => ExecSpAsync("ActualizarCliente", cliente);

    

    public Task<(int RowsAffected, string Mensaje)> AgregarClienteRegimenAsync(NewClienteRegimenRequest regimen)
        => ExecSpAsync("AgregarClienteRegimen", regimen);

    public Task<(int RowsAffected, string Mensaje)> AgregarClienteActividadAsync(NewClienteActividadRequest actividad)
        => ExecSpAsync("AgregarClienteActividad", actividad);

    public Task<(int RowsAffected, string Mensaje)> EliminarClienteAsync(int idCliente)
        => ExecSpAsync("EliminarCliente", new { idCliente });

    public async Task<ClienteResponse?> ObtenerClientePorIdAsync(int idCliente)
    {
        // 1. Usas el helper privado para ejecutar el SP
        using var grid = await QueryMultipleAsync("ObtenerClientePorId", new { idCliente });

        // 2. Mapeas los resulta sets secuencialmente
        var clienteBase = await grid.ReadFirstOrDefaultAsync<ClienteResponse>();
        if (clienteBase is null)
            return null;

        var correos = (await grid.ReadAsync<CorreoElectronico>()).ToList();
        var regimen = await grid.ReadFirstOrDefaultAsync<Regimen>();
        var actividades = (await grid.ReadAsync<Actividad>()).ToList();

        // 3. Ensamblas con 'with'
        return clienteBase with
        {
            Correos = correos,
            Regimen = regimen,
            Actividades = actividades
        };
    }

    public Task<IEnumerable<ClienteSimpleResponse>> ObtenerClientePorNombreOIdentificacionAsync(string busqueda)
        => QueryAsync<ClienteSimpleResponse>("ObtenerClientePorNombreOIdentificacion", new { dato = busqueda });

    public Task<IEnumerable<ClienteSimpleResponse>> ObtenerClientesAsync(string paginado)
        => QueryAsync<ClienteSimpleResponse>("ObtenerClientes", new { paginado });

    public Task<int> ObtenerTotalClientesAsync()
        => QueryFirstOrDefaultAsync<int>("ObtenerTotalClientes");

    public Task<(int RowsAffected, string Mensaje)> AgregarClienteAsync(NewClienteRequest cliente)
    {
        throw new NotImplementedException();
    }

    public async Task<int> AgregarClienteAsync(NewClienteRequest cliente, IDbTransaction transaction)
    {
        var (_, mensaje) = await transaction.ExecuteSpWithOutputAsync("AgregarCliente", cliente);
        return int.Parse(mensaje);
    }
}
