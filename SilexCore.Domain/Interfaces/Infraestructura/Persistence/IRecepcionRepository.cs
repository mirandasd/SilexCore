using SilexCore.Domain.Dtos;

namespace SilexCore.Domain.Interfaces.Infraestructura.Persistence;

public interface IRecepcionRepository
{
    Task<(int RowsAffected, string Mensaje)> AgregarRecepcionAsync(RecepcionRequest recepcion);
    Task<IEnumerable<DocumentosPorNegocioResponse>> ObtenerRecepcionesPorNegocioAsync(int idEntidad);
    Task<RecepcionDetalleResponse?> ObtenerPorIdAsync(int idRecepcionDocumento);
    // ValidarClaveRecepcionDeDocumento es un SELECT plano (sin OUT msj): devuelve
    // el Id de la recepción si la clave ya fue recibida, o null si no existe.
    Task<int?> ValidarClaveRecepcionDeDocumentoAsync(string clave);
}
