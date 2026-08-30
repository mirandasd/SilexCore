using Karin.Persistence.Dapper.Interfaces;
using SilexCore.Domain.Constants;
using SilexCore.Domain.Dtos;
using SilexCore.Domain.Interfaces.Infraestructura.Persistence;

namespace SilexCore.Infrastructure.Persistence;

public class ConsecutivoRepository(ISqlExecutor sqlExecutor) : IConsecutivoRepository
{
    public Task<(int RowsAffected, string Mensaje)> ActualizarConsecutivoAsync(int idConsecutivo, int consecutivo)
        => sqlExecutor.ExecuteSpWithOutputAsync("ActualizarConsecutivo", new { IdConsecutivo = idConsecutivo, Consecutivo = consecutivo }, outputParamName: "msj", connectionName: ConnectionNames.Administracion);

    public Task<(int RowsAffected, string Mensaje)> ActualizarConsecutivoPorRegistroAsync(int idEntidad, int tipo)
        => sqlExecutor.ExecuteSpWithOutputAsync("ActualizarConsecutivoPorRegistro", new { IdEntidad = idEntidad, Tipo = tipo }, outputParamName: "msj", connectionName: ConnectionNames.Administracion);

    public Task<ConsecutivoResponse?> ObtenerConsecutivoPorNegocioAsync(int idEntidad)
        => sqlExecutor.QueryFirstOrDefaultAsync<ConsecutivoResponse?>("ObtenerConsecutivoPorNegocio", new { IdEntidad = idEntidad }, connectionName: ConnectionNames.Administracion);

    public Task<string?> ObtenerConsecutivoPorNegocioYTipoAsync(int idEntidad, int tipo)
        => sqlExecutor.QueryFirstOrDefaultAsync<string?>("ObtenerConsecutivoPorNegocioYTipo", new { IdEntidad = idEntidad, Tipo = tipo }, connectionName: ConnectionNames.Administracion);

}
