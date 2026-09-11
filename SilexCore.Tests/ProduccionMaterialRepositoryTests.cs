using System.Data;
using Karin.Persistence.Dapper.Interfaces;
using Moq;
using SilexCore.Domain.Dtos;
using SilexCore.Domain.Interfaces.Infraestructura.Persistence;
using SilexCore.Infrastructure.Persistence;
using SilexCore.Tests.TestHelpers;

namespace SilexCore.Tests;

public class ProduccionMaterialRepositoryTests
{
    private readonly Mock<ISqlExecutor> _sqlExecutor = new();
    private readonly IProduccionMaterialRepository _repo;

    public ProduccionMaterialRepositoryTests()
    {
        _repo = new ProduccionMaterialRepository(_sqlExecutor.Object);
    }

    [Fact]
    public async Task AgregarAsync_CuandoElSpNoDevuelveNada_DevuelveMensajeDeFallo()
    {
        var registro = new ProduccionMaterialRequest { IdEntidad = 1, Fecha = DateTime.Today };
        _sqlExecutor.Setup(s => s.QueryFirstOrDefaultAsync<AgregarProduccionMaterialResponse>("AgregarProduccionMaterial", registro, It.IsAny<string>(), It.IsAny<CommandType>()))
            .ReturnsAsync((AgregarProduccionMaterialResponse?)null);

        var resultado = await _repo.AgregarAsync(registro);

        Assert.Equal(0, resultado.IdProduccion);
        Assert.Equal("No se pudo registrar la producción.", resultado.Mensaje);
    }

    [Fact]
    public async Task ActualizarAsync_LlamaActualizarProduccionMaterialConElRequestTalCual()
    {
        var registro = new ActualizarProduccionMaterialRequest { IdProduccion = 5, IdEntidad = 1 };
        _sqlExecutor.Setup(s => s.ExecuteSpWithOutputAsync("ActualizarProduccionMaterial", registro, "msj", It.IsAny<string>()))
            .ReturnsAsync((1, "ok"));

        await _repo.ActualizarAsync(registro);

        _sqlExecutor.Verify(s => s.ExecuteSpWithOutputAsync("ActualizarProduccionMaterial", registro, "msj", It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task ObtenerBalanceAsync_EnviaIdEntidadFechaInicioYFechaFinTalCual()
    {
        var inicio = new DateTime(2026, 9, 1);
        var fin = new DateTime(2026, 9, 10);

        await _repo.ObtenerBalanceAsync(idEntidad: 1, fechaInicio: inicio, fechaFin: fin);

        _sqlExecutor.Verify(s => s.QueryAsync<BalanceApiladoResponse>("ObtenerBalanceApiladoPorNegocio",
            It.Is<object>(p =>
                (int)AnonymousObjectAssert.Prop(p, "IdEntidad")! == 1 &&
                (DateTime)AnonymousObjectAssert.Prop(p, "FechaInicio")! == inicio &&
                (DateTime)AnonymousObjectAssert.Prop(p, "FechaFin")! == fin),
            It.IsAny<string>(), It.IsAny<CommandType>()), Times.Once);
    }
}
