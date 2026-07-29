using SilexCore.Domain.Dtos;
using System.Data;

namespace SilexCore.Domain.Interfaces.Infraestructura.Persistence;

public interface IClienteRepository
{
    Task<(int RowsAffected, string Mensaje)> ActivarClienteAsync(int idCliente);
    Task<(int RowsAffected, string Mensaje)> ActualizarClienteAsync(UpdateClienteRequest cliente);
    Task<(int RowsAffected, string Mensaje)> AgregarClienteActividadAsync(NewClienteActividadRequest actividad);
    Task<(int RowsAffected, string Mensaje)> AgregarClienteAsync(NewClienteRequest cliente);
    Task<(int RowsAffected, string Mensaje)> AgregarClienteRegimenAsync(NewClienteRegimenRequest regimen);
    Task<(int RowsAffected, string Mensaje)> EliminarClienteAsync(int idCliente);
    Task<ClienteResponse?> ObtenerClientePorIdAsync(int idCliente);
    Task<IEnumerable<ClienteSimpleResponse>> ObtenerClientePorNombreOIdentificacionAsync(string busqueda);
    Task<IEnumerable<ClienteSimpleResponse>> ObtenerClientesAsync(string paginado);
    Task<int> ObtenerTotalClientesAsync();





    Task<int> AgregarClienteAsync(NewClienteRequest cliente, IDbTransaction transaction);

}
