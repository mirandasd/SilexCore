using SilexCore.Domain.Dtos;

namespace SilexCore.Domain.Interfaces.Infraestructura.Persistence;

public interface IProduccionMaterialRepository
{
    Task<AgregarProduccionMaterialResponse> AgregarAsync(ProduccionMaterialRequest registro);
    Task<(int RowsAffected, string Mensaje)> ActualizarAsync(ActualizarProduccionMaterialRequest registro);
    Task<IEnumerable<ProduccionMaterialResponse>> ObtenerPorNegocioAsync(int idEntidad);
    Task<IEnumerable<BalanceApiladoResponse>> ObtenerBalanceAsync(int idEntidad, DateTime fechaInicio, DateTime fechaFin);
}
