using System.Data;
using Karin.Persistence.Dapper.Interfaces;
using Moq;
using SilexCore.Domain.Dtos;
using SilexCore.Domain.Interfaces.Infraestructura.Persistence;
using SilexCore.Infrastructure.Persistence;
using SilexCore.Tests.TestHelpers;

namespace SilexCore.Tests;

public class OpcionVentaRepositoryTests
{
    private readonly Mock<ISqlExecutor> _sqlExecutor = new();
    private readonly IOpcionVentaRepository _repo;

    public OpcionVentaRepositoryTests()
    {
        _repo = new OpcionVentaRepository(_sqlExecutor.Object);
    }

    [Fact]
    public async Task ObtenerCategoriasAsync_ConsultaObtenerCategoriasDeVenta()
    {
        await _repo.ObtenerCategoriasAsync();

        _sqlExecutor.Verify(s => s.QueryAsync<CategoriaVentaResponse>("ObtenerCategoriasDeVenta", null, It.IsAny<string>(), It.IsAny<CommandType>()), Times.Once);
    }

    [Fact]
    public async Task ObtenerOpcionesPorCategoriaAsync_EnviaIdCategoria()
    {
        await _repo.ObtenerOpcionesPorCategoriaAsync(4);

        _sqlExecutor.Verify(s => s.QueryAsync<OpcionVentaResponse>("ObtenerOpcionesDeVentaPorCategoria",
            It.Is<object>(p => (int)AnonymousObjectAssert.Prop(p, "IdCategoria")! == 4),
            It.IsAny<string>(), It.IsAny<CommandType>()), Times.Once);
    }
}
