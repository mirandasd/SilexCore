using Karin.Persistence.Dapper.Interfaces;
using SilexCore.Domain.Constants;
using SilexCore.Domain.Dtos;
using SilexCore.Domain.Interfaces.Infraestructura.Persistence;

namespace SilexCore.Infrastructure.Persistence;

public class RecepcionRepository(ISqlExecutor sqlExecutor): IRecepcionRepository
{
    public Task<IEnumerable<DocumentosPorNegocioResponse>> ObtenerRecepcionesPorNegocioAsync(int idEntidad)
        => sqlExecutor.QueryAsync<DocumentosPorNegocioResponse>("ObtenerRecepcionPorNegocio", new { IdEntidad = idEntidad }, connectionName: ConnectionNames.Default);

    public Task<int?> ValidarClaveRecepcionDeDocumentoAsync(string clave)
        => sqlExecutor.QueryFirstOrDefaultAsync<int?>("ValidarClaveRecepcionDeDocumento", new { clave }, connectionName: ConnectionNames.Default);

    public Task<(int RowsAffected, string Mensaje)> AgregarRecepcionAsync(RecepcionRequest recepcion)
    => sqlExecutor.ExecuteSpWithOutputAsync("AgregarRecepcionDocumento", recepcion, connectionName: ConnectionNames.Default);

    public Task<RecepcionDetalleResponse?> ObtenerPorIdAsync(int idRecepcionDocumento)
        => sqlExecutor.QueryFirstOrDefaultAsync<RecepcionDetalleResponse?>("ObtenerRecepcionDocumentoPorId", new { IdRecepcionDocumento = idRecepcionDocumento }, connectionName: ConnectionNames.Default);
}