using SilexCore.Domain.Dtos;

namespace SilexCore.Domain.Interfaces.Infraestructura.Persistence;

public interface INotaRepository
{
    Task<RegistrarNotaCreditoResult> RegistrarNotaCreditoAsync(RegistrarNotaCreditoRequest nota);
    Task<IEnumerable<DocumentosPorNegocioResponse>> ObtenerNotasPorNegocioAsync(int idEntidad);
    Task<NotaResponse?> ObtenerNotaPorIdAsync(int idNota);

    Task<(int RowsAffected, string Mensaje)> GuardarHistoricoNotaAsync(
        int idNota, string clave, string pathXml, string pathPdf, string pathRespuesta, int estadoEnvio, int estadoHacienda);
}
