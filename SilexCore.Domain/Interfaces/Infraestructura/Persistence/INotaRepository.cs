using SilexCore.Domain.Dtos;

namespace SilexCore.Domain.Interfaces.Infraestructura.Persistence;

public interface INotaRepository
{
    Task<RegistrarNotaCreditoResult> RegistrarNotaCreditoAsync(RegistrarNotaCreditoRequest nota);
    Task<IEnumerable<DocumentosPorNegocioResponse>> ObtenerNotasPorNegocioAsync(int idEntidad);
    Task<NotaResponse?> ObtenerNotaPorIdAsync(int idNota);
}
