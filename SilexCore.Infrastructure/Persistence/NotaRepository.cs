using Karin.Persistence.Dapper.Interfaces;
using SilexCore.Domain.Constants;
using SilexCore.Domain.Dtos;
using SilexCore.Domain.Interfaces.Infraestructura.Persistence;

namespace SilexCore.Infrastructure.Persistence
{
    public class NotaRepository(ISqlExecutor sqlExecutor): INotaRepository
    {
        private const int TipoNotaCredito = 3; // mismo código que tbl_consecutivo.tipo para "N. Crédito"

        // Consecutivo -> AgregarNotaCredito -> actualizar consecutivo, en una sola
        // transacción (mismo patrón cross-schema que RegistrarFacturaCompletaAsync).
        public Task<RegistrarNotaCreditoResult> RegistrarNotaCreditoAsync(RegistrarNotaCreditoRequest nota)
            => sqlExecutor.ExecuteInTransactionAsync(async tx =>
            {
                var consecutivo = await tx.QueryFirstOrDefaultAsync<string?>("sw_administracion.ObtenerConsecutivoPorNegocioYTipo", new
                {
                    nota.IdEntidad,
                    Tipo = TipoNotaCredito
                });

                if (string.IsNullOrWhiteSpace(consecutivo))
                    throw new InvalidOperationException("No se encontró un consecutivo configurado para notas de crédito en este negocio.");

                var (rowsNota, idNotaStr) = await tx.ExecuteSpWithOutputAsync("AgregarNotaCredito", new
                {
                    idFactura = nota.IdFactura,
                    idAccion = nota.IdAccion,
                    consecutivo,
                    detalle = nota.Detalle ?? string.Empty // tbl_nota.detalle es NOT NULL
                });

                if (rowsNota <= 0 || !int.TryParse(idNotaStr, out var idNota))
                    throw new InvalidOperationException($"No se pudo registrar la nota: {idNotaStr}");

                var (rowsConsecutivo, msjConsecutivo) = await tx.ExecuteSpWithOutputAsync("sw_administracion.ActualizarConsecutivoPorRegistro", new
                {
                    nota.IdEntidad,
                    Tipo = TipoNotaCredito
                });

                if (rowsConsecutivo <= 0)
                    throw new InvalidOperationException($"No se pudo actualizar el consecutivo: {msjConsecutivo}");

                return new RegistrarNotaCreditoResult
                {
                    IdNota = idNota,
                    Consecutivo = consecutivo,
                    Mensaje = "Nota registrada correctamente"
                };
            }, connectionName: ConnectionNames.Default);

        public Task<IEnumerable<DocumentosPorNegocioResponse>> ObtenerNotasPorNegocioAsync(int idEntidad)
            => sqlExecutor.QueryAsync<DocumentosPorNegocioResponse>("ObtenerNotasPorNegocio", new { IdEntidad = idEntidad }, connectionName: ConnectionNames.Default);

        public Task<NotaResponse?> ObtenerNotaPorIdAsync(int idNota)
            => sqlExecutor.QueryFirstOrDefaultAsync<NotaResponse?>("ObtenerNotaPorId", new { idNota }, connectionName: ConnectionNames.Default);

        public Task<(int RowsAffected, string Mensaje)> GuardarHistoricoNotaAsync(
            int idNota, string clave, string pathXml, string pathPdf, string pathRespuesta, int estadoEnvio, int estadoHacienda)
            => sqlExecutor.ExecuteSpWithOutputAsync("SP_AgregarHistoricoNota", new
            {
                idDocumento = idNota,
                Fecha = DateTime.Now,
                Clave = clave,
                PathDocumentoXml = pathXml,
                PathDocumentoPdf = pathPdf,
                PathDocumentoRespuesta = pathRespuesta,
                EstadoEnvio = estadoEnvio,
                EstadoHacienda = estadoHacienda
            }, connectionName: ConnectionNames.Default);
    }
}
