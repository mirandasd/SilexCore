using System.Data;
using Karin.Persistence.Dapper.Interfaces;
using Moq;
using SilexCore.Domain.Interfaces.Infraestructura.Persistence;
using SilexCore.Infrastructure.Persistence;
using SilexCore.Tests.TestHelpers;

namespace SilexCore.Tests;

public class ConsecutivoRepositoryTests
{
    private readonly Mock<ISqlExecutor> _sqlExecutor = new();
    private readonly IConsecutivoRepository _repo;

    public ConsecutivoRepositoryTests()
    {
        _repo = new ConsecutivoRepository(_sqlExecutor.Object);
    }

    [Fact]
    public async Task ActualizarConsecutivoAsync_EnviaIdConsecutivoYConsecutivo()
    {
        _sqlExecutor.Setup(s => s.ExecuteSpWithOutputAsync("ActualizarConsecutivo", It.IsAny<object>(), "msj", It.IsAny<string>()))
            .ReturnsAsync((1, "ok"));

        await _repo.ActualizarConsecutivoAsync(idConsecutivo: 10, consecutivo: 500);

        _sqlExecutor.Verify(s => s.ExecuteSpWithOutputAsync("ActualizarConsecutivo",
            It.Is<object>(p =>
                (int)AnonymousObjectAssert.Prop(p, "IdConsecutivo")! == 10 &&
                (int)AnonymousObjectAssert.Prop(p, "Consecutivo")! == 500),
            "msj", It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task ActualizarConsecutivoPorRegistroAsync_EnviaIdEntidadYTipo()
    {
        _sqlExecutor.Setup(s => s.ExecuteSpWithOutputAsync("ActualizarConsecutivoPorRegistro", It.IsAny<object>(), "msj", It.IsAny<string>()))
            .ReturnsAsync((1, "ok"));

        await _repo.ActualizarConsecutivoPorRegistroAsync(idEntidad: 1, tipo: 2);

        _sqlExecutor.Verify(s => s.ExecuteSpWithOutputAsync("ActualizarConsecutivoPorRegistro",
            It.Is<object>(p =>
                (int)AnonymousObjectAssert.Prop(p, "IdEntidad")! == 1 &&
                (int)AnonymousObjectAssert.Prop(p, "Tipo")! == 2),
            "msj", It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task ObtenerConsecutivoPorNegocioAsync_ConsultaObtenerConsecutivoPorNegocioConIdEntidad()
    {
        await _repo.ObtenerConsecutivoPorNegocioAsync(idEntidad: 3);

        _sqlExecutor.Verify(s => s.QueryFirstOrDefaultAsync<Domain.Dtos.ConsecutivoResponse?>("ObtenerConsecutivoPorNegocio",
            It.Is<object>(p => (int)AnonymousObjectAssert.Prop(p, "IdEntidad")! == 3),
            It.IsAny<string>(), It.IsAny<CommandType>()), Times.Once);
    }

    [Fact]
    public async Task ObtenerConsecutivoPorNegocioYTipoAsync_EnviaIdEntidadYTipo()
    {
        await _repo.ObtenerConsecutivoPorNegocioYTipoAsync(idEntidad: 3, tipo: 1);

        _sqlExecutor.Verify(s => s.QueryFirstOrDefaultAsync<string?>("ObtenerConsecutivoPorNegocioYTipo",
            It.Is<object>(p =>
                (int)AnonymousObjectAssert.Prop(p, "IdEntidad")! == 3 &&
                (int)AnonymousObjectAssert.Prop(p, "Tipo")! == 1),
            It.IsAny<string>(), It.IsAny<CommandType>()), Times.Once);
    }
}
