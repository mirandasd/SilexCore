using Karin.InvoicingService.Client.Interface;
using Refit;
using SilexCore.Domain.Dtos;
using SilexCore.Domain.Interfaces.Infraestructura.Services;

namespace SilexCore.Infrastructure.Services;

public class CertificadoConsultaService(IInvoicingServiceClient invoicingClient) : ICertificadoConsultaService
{
    public async Task<List<EstadoCertificadoResponse>> ObtenerEstadoCertificadosAsync()
    {
        var response = await invoicingClient.ObtenerEstadoCertificadosAsync();

        if (!response.IsSuccessStatusCode || response.Content is null)
        {
            var detalle = response.Error?.Message ?? response.ReasonPhrase ?? "Error desconocido";
            throw new InvalidOperationException($"InvoicingService no pudo obtener el estado de los certificados: {detalle}");
        }

        return response.Content.Select(c => new EstadoCertificadoResponse
        {
            IdEntidad = c.IdEntidad,
            NombreComercial = c.NombreComercial,
            TieneCertificadoRegistrado = c.TieneCertificadoRegistrado,
            ArchivoEncontrado = c.ArchivoEncontrado,
            FechaVencimiento = c.FechaVencimiento,
            DiasRestantes = c.DiasRestantes,
            Vencido = c.Vencido,
            Mensaje = c.Mensaje
        }).ToList();
    }

    public async Task<GuardarCertificadoResponse> SubirCertificadoAsync(int idEntidad, Stream archivo, string nombreArchivo, string contrasenha)
    {
        var parte = new StreamPart(archivo, nombreArchivo, "application/x-pkcs12");
        var response = await invoicingClient.SubirCertificadoAsync(idEntidad, parte, contrasenha);

        if (!response.IsSuccessStatusCode || response.Content is null)
        {
            var detalle = response.Error?.Message ?? response.ReasonPhrase ?? "Error desconocido";
            throw new InvalidOperationException($"InvoicingService no pudo guardar el certificado: {detalle}");
        }

        var c = response.Content;
        return new GuardarCertificadoResponse
        {
            IdEntidad = c.IdEntidad,
            NombreArchivo = c.NombreArchivo,
            FechaVencimiento = c.FechaVencimiento,
            Mensaje = c.Mensaje
        };
    }
}
