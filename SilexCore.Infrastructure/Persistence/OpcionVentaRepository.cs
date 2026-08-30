using Karin.Persistence.Dapper.Interfaces;
using SilexCore.Domain.Constants;
using SilexCore.Domain.Dtos;
using SilexCore.Domain.Interfaces.Infraestructura.Persistence;

namespace SilexCore.Infrastructure.Persistence;

public class OpcionVentaRepository(ISqlExecutor sqlExecutor) : IOpcionVentaRepository
{
    public Task<IEnumerable<CategoriaVentaResponse>> ObtenerCategoriasAsync()
        => sqlExecutor.QueryAsync<CategoriaVentaResponse>("ObtenerCategoriasDeVenta", connectionName: ConnectionNames.Administracion);

    public Task<IEnumerable<OpcionVentaResponse>> ObtenerOpcionesPorCategoriaAsync(int idCategoria)
        => sqlExecutor.QueryAsync<OpcionVentaResponse>("ObtenerOpcionesDeVentaPorCategoria", new { IdCategoria = idCategoria }, connectionName: ConnectionNames.Administracion);
}
