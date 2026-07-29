using SilexCore.Domain.Dtos;

namespace SilexCore.Domain.Interfaces.Infraestructura.Persistence;

public interface IConsecutivoRepository
{
    Task<(int RowsAffected, string Mensaje)> ActualizarConsecutivoAsync(int idConsecutivo);
    Task<(int RowsAffected, string Mensaje)> ActualizarConsecutivoPorRegistroAsync(int idEntidad, int tipo);
    Task<ConsecutivoResponse> ObtenerConsecutivoPorNegocioAsync(int idEntidad);
    Task<string> ObtenerConsecutivoPorNegocioYTipoAsync(int idEntidad, int tipo);
}
