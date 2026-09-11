using SilexCore.Domain.Dtos;
using SilexCore.Domain.Interfaces.Infraestructura.Persistence;
using SilexCore.Infrastructure.Persistence;
using SilexCore.Tests.TestHelpers;
using Karin.Persistence.Dapper.Interfaces;
using Moq;

namespace SilexCore.Tests;

// Cada repositorio de SilexCore arma el nombre del SP y sus parámetros a mano
// como objeto anónimo -- exactamente lo que ya rompió AgregarUsuario y
// ObtenerPerfilUsuarioPorEmail (parámetro con nombre distinto al esperado por
// el SP). Estas pruebas fijan, por reflexión, el nombre del SP y de cada
// parámetro que de verdad importa, para que un cambio accidental de nombre
// falle aquí en vez de en producción.
public class ClienteRepositoryTests
{
    private readonly Mock<ISqlExecutor> _sqlExecutor = new();
    private readonly IClienteRepository _repo;

    public ClienteRepositoryTests()
    {
        _repo = new ClienteRepository(_sqlExecutor.Object);
    }

    [Fact]
    public async Task ActivarClienteAsync_LlamaActivarClienteConIdCliente()
    {
        _sqlExecutor.Setup(s => s.ExecuteSpWithOutputAsync("ActivarCliente", It.IsAny<object>(), "msj", It.IsAny<string>()))
            .ReturnsAsync((1, "ok"));

        await _repo.ActivarClienteAsync(42);

        _sqlExecutor.Verify(s => s.ExecuteSpWithOutputAsync("ActivarCliente",
            It.Is<object>(p => (int)AnonymousObjectAssert.Prop(p, "IdCliente")! == 42),
            "msj", It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task EliminarClienteAsync_LlamaEliminarClienteConIdCliente()
    {
        _sqlExecutor.Setup(s => s.ExecuteSpWithOutputAsync("EliminarCliente", It.IsAny<object>(), "msj", It.IsAny<string>()))
            .ReturnsAsync((1, "ok"));

        await _repo.EliminarClienteAsync(7);

        _sqlExecutor.Verify(s => s.ExecuteSpWithOutputAsync("EliminarCliente",
            It.Is<object>(p => (int)AnonymousObjectAssert.Prop(p, "IdCliente")! == 7),
            "msj", It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task ActualizarClienteAsync_LlamaActualizarClienteConElRequestTalCual()
    {
        var request = new UpdateClienteRequest { IdCliente = 5, Nombre = "Ferretería del Valle" };
        _sqlExecutor.Setup(s => s.ExecuteSpWithOutputAsync("ActualizarCliente", request, "msj", It.IsAny<string>()))
            .ReturnsAsync((1, "ok"));

        await _repo.ActualizarClienteAsync(request);

        _sqlExecutor.Verify(s => s.ExecuteSpWithOutputAsync("ActualizarCliente", request, "msj", It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task AgregarClienteRegimenAsync_LlamaAgregarClienteRegimen()
    {
        var regimen = new NewClienteRegimenRequest { IdCliente = 1, Codigo = 2 };
        _sqlExecutor.Setup(s => s.ExecuteSpWithOutputAsync("AgregarClienteRegimen", regimen, "msj", It.IsAny<string>()))
            .ReturnsAsync((1, "ok"));

        await _repo.AgregarClienteRegimenAsync(regimen);

        _sqlExecutor.Verify(s => s.ExecuteSpWithOutputAsync("AgregarClienteRegimen", regimen, "msj", It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task AgregarClienteActividadAsync_LlamaAgregarClienteActividad()
    {
        var actividad = new NewClienteActividadRequest { IdCliente = 1, Codigo = "A1" };
        _sqlExecutor.Setup(s => s.ExecuteSpWithOutputAsync("AgregarClienteActividad", actividad, "msj", It.IsAny<string>()))
            .ReturnsAsync((1, "ok"));

        await _repo.AgregarClienteActividadAsync(actividad);

        _sqlExecutor.Verify(s => s.ExecuteSpWithOutputAsync("AgregarClienteActividad", actividad, "msj", It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task ObtenerClientePorIdAsync_CuandoNoExisteElCliente_NoConsultaRegimenNiActividades()
    {
        _sqlExecutor.Setup(s => s.QueryFirstOrDefaultAsync<ClienteResponse>("ObtenerClientePorId", It.IsAny<object>(), It.IsAny<string>(), It.IsAny<System.Data.CommandType>()))
            .ReturnsAsync((ClienteResponse?)null);

        var resultado = await _repo.ObtenerClientePorIdAsync(99);

        Assert.Null(resultado);
        _sqlExecutor.Verify(s => s.QueryAsync<Regimen>(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string>(), It.IsAny<System.Data.CommandType>()), Times.Never);
    }

    [Fact]
    public async Task ObtenerClientePorIdAsync_CuandoExiste_CombinaClienteConRegimenYActividades()
    {
        _sqlExecutor.Setup(s => s.QueryFirstOrDefaultAsync<ClienteResponse>("ObtenerClientePorId",
                It.Is<object>(p => (int)AnonymousObjectAssert.Prop(p, "idCliente")! == 3),
                It.IsAny<string>(), It.IsAny<System.Data.CommandType>()))
            .ReturnsAsync(new ClienteResponse { IdCliente = 3, Nombre = "Kerlin" });

        _sqlExecutor.Setup(s => s.QueryAsync<Regimen>("ObtenerClienteRegimen", It.IsAny<object>(), It.IsAny<string>(), It.IsAny<System.Data.CommandType>()))
            .ReturnsAsync([new Regimen { Codigo = 1, Descripcion = "Tradicional" }]);
        _sqlExecutor.Setup(s => s.QueryAsync<Actividad>("ObtenerClienteActividades", It.IsAny<object>(), It.IsAny<string>(), It.IsAny<System.Data.CommandType>()))
            .ReturnsAsync([new Actividad { Codigo = "A1" }]);

        var resultado = await _repo.ObtenerClientePorIdAsync(3);

        Assert.NotNull(resultado);
        Assert.Equal("Tradicional", resultado!.Regimen?.Descripcion);
        Assert.Single(resultado.Actividades!);
    }

    [Fact]
    public async Task AgregarClienteAsync_CuandoElSpNoDevuelveNada_DevuelveMensajeDeFallo()
    {
        var nuevo = new NewClienteRequest { Nombre = "Cliente Nuevo" };
        _sqlExecutor.Setup(s => s.QueryFirstOrDefaultAsync<AgregarClienteResponse>("AgregarCliente", nuevo, It.IsAny<string>(), It.IsAny<System.Data.CommandType>()))
            .ReturnsAsync((AgregarClienteResponse?)null);

        var resultado = await _repo.AgregarClienteAsync(nuevo);

        Assert.Equal(0, resultado.IdCliente);
        Assert.Equal("No se pudo agregar el cliente.", resultado.Mensaje);
    }
}
