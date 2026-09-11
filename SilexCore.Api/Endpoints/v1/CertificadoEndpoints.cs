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
        .WithName("ObtenerEstadoCertificados")
        .RequireAuthorization("RequiereRolAdmin");

        // Sube/reemplaza el certificado de un negocio -- solo Admin, es la llave
        // de firma legal del negocio.
        app.MapPost("/api/certificados/{idEntidad:int}", async (int idEntidad, HttpRequest request, ICertificadoConsultaService service) =>
        {
            if (!request.HasFormContentType)
                return Results.BadRequest(new { mensaje = "Se esperaba un formulario multipart/form-data." });

            var form = await request.ReadFormAsync();
            var archivo = form.Files["archivo"];
            var contrasenha = form["contrasenha"].ToString();

            if (archivo is null)
                return Results.BadRequest(new { mensaje = "Falta el archivo del certificado." });
            if (string.IsNullOrWhiteSpace(contrasenha))
                return Results.BadRequest(new { mensaje = "Falta la contraseña del certificado." });

            try
            {
                await using var stream = archivo.OpenReadStream();
                var resultado = await service.SubirCertificadoAsync(idEntidad, stream, archivo.FileName, contrasenha);
                return Results.Ok(resultado);
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { mensaje = ex.Message });
            }
        })
        .WithName("SubirCertificado")
        .RequireAuthorization("RequiereRolAdmin")
        .DisableAntiforgery();
    }
}
