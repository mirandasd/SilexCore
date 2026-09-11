using Karin.Persistence.Dapper.Interfaces;
using SilexCore.Domain.Constants;
using SilexCore.Domain.Dtos;
using SilexCore.Domain.Interfaces.Infraestructura.Persistence;

namespace SilexCore.Infrastructure.Persistence;

public class UsuarioRepository(ISqlExecutor sqlExecutor) : IUsuarioRepository
{
    public Task<(int RowsAffected, string Mensaje)> AgregarUsuarioAsync(string authUserId, RegistrarUsuarioRequest usuario)
        => sqlExecutor.ExecuteSpWithOutputAsync("AgregarUsuario", new
        {
            AuthUserId = authUserId,
            usuario.IdEntidad,
            usuario.NombrePersona,
            pEmail = usuario.Email,
            usuario.Rol,
            usuario.Telefono,
            usuario.Cargo
        }, connectionName: ConnectionNames.Administracion);

    public Task<(int RowsAffected, string Mensaje)> ActualizarPerfilAsync(ActualizarPerfilRequest perfil)
        => sqlExecutor.ExecuteSpWithOutputAsync("ActualizarPerfilUsuario", perfil, connectionName: ConnectionNames.Administracion);

    public Task<PerfilUsuarioResponse?> ObtenerPerfilPorIdAsync(int idUsuario)
        => sqlExecutor.QueryFirstOrDefaultAsync<PerfilUsuarioResponse>("ObtenerPerfilUsuarioPorId", new { IdUsuario = idUsuario }, connectionName: ConnectionNames.Administracion);

    public Task<PerfilUsuarioResponse?> ObtenerPerfilPorEmailAsync(string email)
        => sqlExecutor.QueryFirstOrDefaultAsync<PerfilUsuarioResponse>("ObtenerPerfilUsuarioPorEmail", new { pEmail = email }, connectionName: ConnectionNames.Administracion);

    public Task<IEnumerable<PerfilUsuarioResponse>> ListarUsuariosAsync()
        => sqlExecutor.QueryAsync<PerfilUsuarioResponse>("ObtenerUsuarios", connectionName: ConnectionNames.Administracion);

    public Task<IEnumerable<NegocioResponse>> ListarNegociosAsync()
        => sqlExecutor.QueryAsync<NegocioResponse>("ObtenerNegocioParaSistema", connectionName: ConnectionNames.Administracion);

    public Task<EntidadReporteResponse?> ObtenerEntidadReporteAsync(int idEntidad)
        => sqlExecutor.QueryFirstOrDefaultAsync<EntidadReporteResponse?>("ObtenerEntidadReportePorId", new { IdEntidad = idEntidad }, connectionName: ConnectionNames.Administracion);

    public Task<ConfiguracionDocumentoResponse?> ObtenerConfiguracionDocumentoAsync()
        => sqlExecutor.QueryFirstOrDefaultAsync<ConfiguracionDocumentoResponse?>("ObtenerConfiguracionDocumento", connectionName: ConnectionNames.Administracion);
}
