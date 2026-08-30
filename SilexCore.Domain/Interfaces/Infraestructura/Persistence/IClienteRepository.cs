using SilexCore.Domain.Dtos;

namespace SilexCore.Domain.Interfaces.Infraestructura.Persistence;

public interface IClienteRepository
{
    Task<(int RowsAffected, string Mensaje)> ActivarClienteAsync(int idCliente);
    Task<(int RowsAffected, string Mensaje)> ActualizarClienteAsync(UpdateClienteRequest cliente);
    Task<(int RowsAffected, string Mensaje)> AgregarClienteActividadAsync(NewClienteActividadRequest actividad);
    Task<AgregarClienteResponse> AgregarClienteAsync(NewClienteRequest cliente);
    Task<(int RowsAffected, string Mensaje)> AgregarClienteRegimenAsync(NewClienteRegimenRequest regimen);
    Task<(int RowsAffected, string Mensaje)> EliminarClienteAsync(int idCliente);
    Task<ClienteResponse?> ObtenerClientePorIdAsync(int idCliente);
    Task<IEnumerable<ClienteResponse>> ObtenerClientePorNombreOIdentificacionAsync(string busqueda);
    Task<IEnumerable<ClienteResponse>> ObtenerClientesAsync();
    Task<int> ObtenerTotalClientesAsync();

}
