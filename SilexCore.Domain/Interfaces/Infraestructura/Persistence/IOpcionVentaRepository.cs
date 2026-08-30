using SilexCore.Domain.Dtos;

namespace SilexCore.Domain.Interfaces.Infraestructura.Persistence;

public interface IOpcionVentaRepository
{
    Task<IEnumerable<CategoriaVentaResponse>> ObtenerCategoriasAsync();
    Task<IEnumerable<OpcionVentaResponse>> ObtenerOpcionesPorCategoriaAsync(int idCategoria);
}
