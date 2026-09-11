using SilexCore.Domain.Dtos;

namespace SilexCore.Domain.Interfaces.Infraestructura.Services;

public interface ICertificadoConsultaService
{
    Task<List<EstadoCertificadoResponse>> ObtenerEstadoCertificadosAsync();
}
