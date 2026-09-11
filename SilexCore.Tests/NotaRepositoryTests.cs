using System.Data;
using Karin.Persistence.Dapper.Interfaces;
using Moq;
using SilexCore.Domain.Dtos;
using SilexCore.Domain.Interfaces.Infraestructura.Persistence;
using SilexCore.Infrastructure.Persistence;
using SilexCore.Tests.TestHelpers;

namespace SilexCore.Tests;

public class NotaRepositoryTests
{
    private readonly Mock<ISqlExecutor> _sqlExecutor = new();
    private readonly INotaRepository _repo;

    public NotaRepositoryTests()
    {
        _repo = new NotaRepository(_sqlExecutor.Object);

        _sqlExecutor
            .Setup(s => s.ExecuteInTransactionAsync(It.IsAny<Func<ISqlExecutor, Task<RegistrarNotaCreditoResult>>>(), It.IsAny<string>(), It.IsAny<IsolationLevel>()))
            .Returns<Func<ISqlExecutor, Task<RegistrarNotaCreditoResult>>, string, IsolationLevel>((operacion, _, _) => operacion(_sqlExecutor.Object));
    }

    private static RegistrarNotaCreditoRequest RequestBase() => new()
    {
        IdEntidad = 1,
        IdFactura = 500,
        IdAccion = 1,
        Detalle = "Anulación de factura"
    };

    [Fact]
    public async Task RegistrarNotaCreditoAsync_PideElConsecutivoParaElTipoNotaCredito()
    {
        _sqlExecutor.Setup(s => s.QueryFirstOrDefaultAsync<string?>("sw_administracion.ObtenerConsecutivoPorNegocioYTipo", It.IsAny<object>(), It.IsAny<string>(), It.IsAny<CommandType>()))
            .ReturnsAsync("00100003010000000001");
        _sqlExecutor.Setup(s => s.ExecuteSpWithOutputAsync("AgregarNotaCredito", It.IsAny<object>(), "msj", It.IsAny<string>()))
            .ReturnsAsync((1, "42"));
        _sqlExecutor.Setup(s => s.ExecuteSpWithOutputAsync("sw_administracion.ActualizarConsecutivoPorRegistro", It.IsAny<object>(), "msj", It.IsAny<string>()))
            .ReturnsAsync((1, "ok"));

        var resultado = await _repo.RegistrarNotaCreditoAsync(RequestBase());

        Assert.Equal(42, resultado.IdNota);
        Assert.Equal("00100003010000000001", resultado.Consecutivo);
        _sqlExecutor.Verify(s => s.QueryFirstOrDefaultAsync<string?>("sw_administracion.ObtenerConsecutivoPorNegocioYTipo",
            It.Is<object>(p => (int)AnonymousObjectAssert.Prop(p, "Tipo")! == 3), // 3 = tipo consecutivo de N. Crédito
            It.IsAny<string>(), It.IsAny<CommandType>()), Times.Once);
    }

    [Fact]
    public async Task RegistrarNotaCreditoAsync_SinConsecutivoConfigurado_Lanza()
    {
        _sqlExecutor.Setup(s => s.QueryFirstOrDefaultAsync<string?>("sw_administracion.ObtenerConsecutivoPorNegocioYTipo", It.IsAny<object>(), It.IsAny<string>(), It.IsAny<CommandType>()))
            .ReturnsAsync((string?)null);

        await Assert.ThrowsAsync<InvalidOperationException>(() => _repo.RegistrarNotaCreditoAsync(RequestBase()));
    }

    [Fact]
    public async Task RegistrarNotaCreditoAsync_FallaAlRegistrarLaNota_Lanza()
    {
        _sqlExecutor.Setup(s => s.QueryFirstOrDefaultAsync<string?>("sw_administracion.ObtenerConsecutivoPorNegocioYTipo", It.IsAny<object>(), It.IsAny<string>(), It.IsAny<CommandType>()))
            .ReturnsAsync("00100003010000000001");
        _sqlExecutor.Setup(s => s.ExecuteSpWithOutputAsync("AgregarNotaCredito", It.IsAny<object>(), "msj", It.IsAny<string>()))
            .ReturnsAsync((0, "Error"));

        await Assert.ThrowsAsync<InvalidOperationException>(() => _repo.RegistrarNotaCreditoAsync(RequestBase()));
    }

    [Fact]
    public async Task RegistrarNotaCreditoAsync_FallaActualizarConsecutivoAlFinal_Lanza()
    {
        _sqlExecutor.Setup(s => s.QueryFirstOrDefaultAsync<string?>("sw_administracion.ObtenerConsecutivoPorNegocioYTipo", It.IsAny<object>(), It.IsAny<string>(), It.IsAny<CommandType>()))
            .ReturnsAsync("00100003010000000001");
        _sqlExecutor.Setup(s => s.ExecuteSpWithOutputAsync("AgregarNotaCredito", It.IsAny<object>(), "msj", It.IsAny<string>()))
            .ReturnsAsync((1, "42"));
        _sqlExecutor.Setup(s => s.ExecuteSpWithOutputAsync("sw_administracion.ActualizarConsecutivoPorRegistro", It.IsAny<object>(), "msj", It.IsAny<string>()))
            .ReturnsAsync((0, "Error"));

        await Assert.ThrowsAsync<InvalidOperationException>(() => _repo.RegistrarNotaCreditoAsync(RequestBase()));
    }

    [Fact]
    public async Task ObtenerNotaPorIdAsync_EnviaIdNota()
    {
        await _repo.ObtenerNotaPorIdAsync(8);

        _sqlExecutor.Verify(s => s.QueryFirstOrDefaultAsync<NotaResponse?>("ObtenerNotaPorId",
            It.Is<object>(p => (int)AnonymousObjectAssert.Prop(p, "idNota")! == 8),
            It.IsAny<string>(), It.IsAny<CommandType>()), Times.Once);
    }
}
