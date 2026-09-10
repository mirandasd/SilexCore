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

    public Task<(int RowsAffected, string Mensaje)> GuardarHistoricoRecepcionAsync(
        int idRecepcion, string consecutivo, string pathXml, string pathPdf, string pathRespuesta, int estadoEnvio, int estadoHacienda)
        => sqlExecutor.ExecuteSpWithOutputAsync("SP_AgregarHistoricoRecepcion", new
        {
            idDocumento = idRecepcion,
            Fecha = DateTime.Now,
            Consecutivo = consecutivo,
            PathDocumentoXml = pathXml,
            PathDocumentoPdf = pathPdf,
            PathDocumentoRespuesta = pathRespuesta,
            EstadoEnvio = estadoEnvio,
            EstadoHacienda = estadoHacienda
        }, connectionName: ConnectionNames.Default);
}