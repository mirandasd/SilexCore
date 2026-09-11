using System.Data;
using Karin.Persistence.Dapper.Interfaces;
using Moq;
using SilexCore.Domain.Dtos;
using SilexCore.Domain.Interfaces.Infraestructura.Persistence;
using SilexCore.Infrastructure.Persistence;
using SilexCore.Tests.TestHelpers;

namespace SilexCore.Tests;

// AgregarUsuario y ObtenerPerfilUsuarioPorEmail ya se rompieron una vez por un
// parámetro con el mismo nombre que su columna (email/Email). El fix fue
// renombrar el parámetro del SP a "pEmail" -- estas pruebas fijan que el
// repositorio también use "pEmail" al llamar, para que un futuro refactor que
// "limpie" el nombre de vuelta a "Email" falle aquí antes que en producción.
public class UsuarioRepositoryTests
{
    private readonly Mock<ISqlExecutor> _sqlExecutor = new();
    private readonly IUsuarioRepository _repo;

    public UsuarioRepositoryTests()
    {
        _repo = new UsuarioRepository(_sqlExecutor.Object);
    }

    [Fact]
    public async Task AgregarUsuarioAsync_EnviaElCorreoComoPEmail_NoComoEmail()
    {
        var usuario = new RegistrarUsuarioRequest
        {
            IdEntidad = 1,
            NombrePersona = "Cajero Prueba",
            Email = "cajero@silexcore.test",
            Rol = "Cajero"
        };
        _sqlExecutor.Setup(s => s.ExecuteSpWithOutputAsync("AgregarUsuario", It.IsAny<object>(), "msj", It.IsAny<string>()))
            .ReturnsAsync((1, "Usuario registrado correctamente"));

        await _repo.AgregarUsuarioAsync("auth-guid-123", usuario);

        _sqlExecutor.Verify(s => s.ExecuteSpWithOutputAsync("AgregarUsuario",
            It.Is<object>(p =>
                (string)AnonymousObjectAssert.Prop(p, "pEmail")! == "cajero@silexcore.test" &&
                (string)AnonymousObjectAssert.Prop(p, "AuthUserId")! == "auth-guid-123" &&
                !AnonymousObjectAssert.TieneProp(p, "Email")),
            "msj", It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task ObtenerPerfilPorEmailAsync_EnviaElCorreoComoPEmail_NoComoEmail()
    {
        await _repo.ObtenerPerfilPorEmailAsync("diego@silexcore.test");

        _sqlExecutor.Verify(s => s.QueryFirstOrDefaultAsync<PerfilUsuarioResponse>("ObtenerPerfilUsuarioPorEmail",
            It.Is<object>(p =>
                (string)AnonymousObjectAssert.Prop(p, "pEmail")! == "diego@silexcore.test" &&
                !AnonymousObjectAssert.TieneProp(p, "Email")),
            It.IsAny<string>(), It.IsAny<CommandType>()), Times.Once);
    }

    [Fact]
    public async Task ObtenerPerfilPorIdAsync_EnviaIdUsuario()
    {
        await _repo.ObtenerPerfilPorIdAsync(9);

        _sqlExecutor.Verify(s => s.QueryFirstOrDefaultAsync<PerfilUsuarioResponse>("ObtenerPerfilUsuarioPorId",
            It.Is<object>(p => (int)AnonymousObjectAssert.Prop(p, "IdUsuario")! == 9),
            It.IsAny<string>(), It.IsAny<CommandType>()), Times.Once);
    }

    [Fact]
    public async Task ActualizarPerfilAsync_LlamaActualizarPerfilUsuarioConElRequestTalCual()
    {
        var perfil = new ActualizarPerfilRequest { IdUsuario = 1, Nombre = "Diego" };
        _sqlExecutor.Setup(s => s.ExecuteSpWithOutputAsync("ActualizarPerfilUsuario", perfil, "msj", It.IsAny<string>()))
            .ReturnsAsync((1, "ok"));

        await _repo.ActualizarPerfilAsync(perfil);

        _sqlExecutor.Verify(s => s.ExecuteSpWithOutputAsync("ActualizarPerfilUsuario", perfil, "msj", It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task ListarUsuariosAsync_ConsultaObtenerUsuarios()
    {
        await _repo.ListarUsuariosAsync();

        _sqlExecutor.Verify(s => s.QueryAsync<PerfilUsuarioResponse>("ObtenerUsuarios", null, It.IsAny<string>(), It.IsAny<CommandType>()), Times.Once);
    }

    [Fact]
    public async Task ListarNegociosAsync_ConsultaObtenerNegocioParaSistema()
    {
        await _repo.ListarNegociosAsync();

        _sqlExecutor.Verify(s => s.QueryAsync<NegocioResponse>("ObtenerNegocioParaSistema", null, It.IsAny<string>(), It.IsAny<CommandType>()), Times.Once);
    }
}
