using SilexCore.Domain.Dtos;

namespace SilexCore.Domain.Interfaces.Infraestructura.Persistence;

public interface IUsuarioRepository
{
    Task<(int RowsAffected, string Mensaje)> AgregarUsuarioAsync(string authUserId, RegistrarUsuarioRequest usuario);
    Task<(int RowsAffected, string Mensaje)> ActualizarPerfilAsync(ActualizarPerfilRequest perfil);
    Task<PerfilUsuarioResponse?> ObtenerPerfilPorIdAsync(int idUsuario);
    Task<PerfilUsuarioResponse?> ObtenerPerfilPorEmailAsync(string email);
    Task<IEnumerable<PerfilUsuarioResponse>> ListarUsuariosAsync();
    Task<IEnumerable<NegocioResponse>> ListarNegociosAsync();
    Task<EntidadReporteResponse?> ObtenerEntidadReporteAsync(int idEntidad);
    Task<ConfiguracionDocumentoResponse?> ObtenerConfiguracionDocumentoAsync();
}
