using SilexCore.Api.Interfaces;
using SilexCore.Domain.Interfaces.Infraestructura.Services;

namespace SilexCore.Api.Endpoints.v1;

public class CertificadoEndpoints : IEndpointModule
{
    public void RegistrarEndpoints(IEndpointRouteBuilder app)
    {
        // Estado de vencimiento de los certificados de firma de todos los negocios
        // activos -- lo consume la tarjeta de certificados del index de silex-app.
        app.MapGet("/api/certificados/estado", async (ICertificadoConsultaService service) =>
        {
            var resultado = await service.ObtenerEstadoCertificadosAsync();
            return Results.Ok(resultado);
        })
        .WithName("ObtenerEstadoCertificados");
    }
}
