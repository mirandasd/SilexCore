using Karin.Persistence.Dapper.Interfaces;
using SilexCore.Domain.Constants;
using SilexCore.Domain.Dtos;
using SilexCore.Domain.Interfaces.Infraestructura.Persistence;

namespace SilexCore.Infrastructure.Persistence;

public class ProduccionMaterialRepository(ISqlExecutor sqlExecutor) : IProduccionMaterialRepository
{
    public async Task<AgregarProduccionMaterialResponse> AgregarAsync(ProduccionMaterialRequest registro)
        => await sqlExecutor.QueryFirstOrDefaultAsync<AgregarProduccionMaterialResponse>("AgregarProduccionMaterial", registro, connectionName: ConnectionNames.Default)
           ?? new AgregarProduccionMaterialResponse { IdProduccion = 0, Mensaje = "No se pudo registrar la producción." };

    public Task<(int RowsAffected, string Mensaje)> ActualizarAsync(ActualizarProduccionMaterialRequest registro)
        => sqlExecutor.ExecuteSpWithOutputAsync("ActualizarProduccionMaterial", registro, connectionName: ConnectionNames.Default);

    public Task<IEnumerable<ProduccionMaterialResponse>> ObtenerPorNegocioAsync(int idEntidad)
        => sqlExecutor.QueryAsync<ProduccionMaterialResponse>("ObtenerProduccionMaterialPorNegocio", new { IdEntidad = idEntidad }, connectionName: ConnectionNames.Default);

    public Task<IEnumerable<BalanceApiladoResponse>> ObtenerBalanceAsync(int idEntidad, DateTime fechaInicio, DateTime fechaFin)
        => sqlExecutor.QueryAsync<BalanceApiladoResponse>("ObtenerBalanceApiladoPorNegocio", new { IdEntidad = idEntidad, FechaInicio = fechaInicio, FechaFin = fechaFin }, connectionName: ConnectionNames.Default);
}
