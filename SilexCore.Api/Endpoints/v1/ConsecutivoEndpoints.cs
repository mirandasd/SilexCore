using SilexCore.Api.Interfaces;
using SilexCore.Domain.Dtos;
using SilexCore.Domain.Interfaces.Infraestructura.Persistence;

namespace SilexCore.Api.Endpoints.v1;

public class ConsecutivoEndpoints : IEndpointModule
{
    public void RegistrarEndpoints(IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup("/api/consecutivo")
                   .WithTags("Consecutivos") // Categoría bonita para Swagger / OpenAPI
                   .RequireAuthorization();

        // GET: /api/consecutivo/negocio/5
        grupo.MapGet("/negocio/{idEntidad:int}", async (int idEntidad, IConsecutivoRepository repo) =>
        {
            var resultado = await repo.ObtenerConsecutivoPorNegocioAsync(idEntidad);
            return resultado is not null ? Results.Ok(resultado) : Results.NotFound();
        })
        .WithName("ObtenerConsecutivoPorNegocio");

        // GET: /api/consecutivo/negocio/5/tipo/1
        grupo.MapGet("/negocio/{idEntidad:int}/tipo/{tipo:int}", async (int idEntidad, int tipo, IConsecutivoRepository repo) =>
        {
            var consecutivo = await repo.ObtenerConsecutivoPorNegocioYTipoAsync(idEntidad, tipo);
            return consecutivo is not null ? Results.Ok(consecutivo) : Results.NotFound();
        })
        .WithName("ObtenerConsecutivoPorNegocioYTipo");

        // PUT: /api/consecutivo/5
        grupo.MapPut("/{idConsecutivo:int}", async (int idConsecutivo, ActualizarConsecutivoRequest body, IConsecutivoRepository repo) =>
        {
            var (rowsAffected, mensaje) = await repo.ActualizarConsecutivoAsync(idConsecutivo, body.Consecutivo);

            return rowsAffected > 0
                ? Results.Ok(new { Mensaje = mensaje, FilasAfectadas = rowsAffected })
                : Results.BadRequest(new { Mensaje = mensaje });
        })
        .WithName("ActualizarConsecutivo");

        // PUT: /api/consecutivo/negocio/5/tipo/1
        grupo.MapPut("/negocio/{idEntidad:int}/tipo/{tipo:int}", async (int idEntidad, int tipo, IConsecutivoRepository repo) =>
        {
            var (rowsAffected, mensaje) = await repo.ActualizarConsecutivoPorRegistroAsync(idEntidad, tipo);

            return rowsAffected > 0
                ? Results.Ok(new { Mensaje = mensaje, FilasAfectadas = rowsAffected })
                : Results.BadRequest(new { Mensaje = mensaje });
        })
        .WithName("ActualizarConsecutivoPorRegistro");
    }
}
