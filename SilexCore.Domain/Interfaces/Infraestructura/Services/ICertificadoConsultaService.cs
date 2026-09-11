using SilexCore.Domain.Dtos;

namespace SilexCore.Domain.Interfaces.Infraestructura.Services;

public interface ICertificadoConsultaService
{
    Task<List<EstadoCertificadoResponse>> ObtenerEstadoCertificadosAsync();
    Task<GuardarCertificadoResponse> SubirCertificadoAsync(int idEntidad, Stream archivo, string nombreArchivo, string contrasenha);
}
