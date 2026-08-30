using SilexCore.Api.Interfaces;
using SilexCore.Domain.Interfaces.Infraestructura.Persistence;

namespace SilexCore.Api.Endpoints.v1;

// Catálogos de solo lectura usados por varias pantallas (selector de negocio,
// armado del detalle de factura, etc.). Cualquier usuario autenticado puede
// consultarlos -- no requieren rol de administrador, a diferencia de
// /api/usuario/negocios (que existe para el flujo de alta de usuarios).
public class CatalogoEndpoints : IEndpointModule
{
    public void RegistrarEndpoints(IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup("/api/catalogo")
                       .WithTags("Catalogos")
                       .RequireAuthorization();

        grupo.MapGet("/negocios", async (IUsuarioRepository usuarioRepo) =>
            Results.Ok(await usuarioRepo.ListarNegociosAsync()))
        .WithName("ListarNegociosCatalogo");

        grupo.MapGet("/categorias-venta", async (IOpcionVentaRepository repo) =>
            Results.Ok(await repo.ObtenerCategoriasAsync()))
        .WithName("ListarCategoriasVenta");

        grupo.MapGet("/opciones-venta/{idCategoria:int}", async (int idCategoria, IOpcionVentaRepository repo) =>
            Results.Ok(await repo.ObtenerOpcionesPorCategoriaAsync(idCategoria)))
        .WithName("ListarOpcionesVenta");
    }
}
