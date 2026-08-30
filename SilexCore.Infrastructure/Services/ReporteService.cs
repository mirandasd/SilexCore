using Karin.ReportingService.Client.Dtos;
using Karin.ReportingService.Client.Interface;
using SilexCore.Domain.Interfaces.Infraestructura.Services;

namespace SilexCore.Infrastructure.Services;

public class ReporteService(IReportingServiceClient client) : IReporteService
{
    public async Task<(byte[] Contenido, string ContentType)> GenerarReporteAsync(string tipoReporte, object datos, string formato = "pdf")
    {
        var response = await client.GenerarReporteAsync(new GenerarReporteRequest(tipoReporte, datos, formato));
        response.EnsureSuccessStatusCode();

        var contentType = response.Content.Headers.ContentType?.MediaType ?? "application/octet-stream";
        var contenido = await response.Content.ReadAsByteArrayAsync();

        return (contenido, contentType);
    }
}
