using SilexCore.Api.Interfaces;
using SilexCore.Domain.Dtos;
using SilexCore.Domain.Interfaces.Infraestructura.Persistence;

namespace SilexCore.Api.Endpoints.v1
{
    public class ClienteEndpoints : IEndpointModule
    {
        public void RegistrarEndpoints(IEndpointRouteBuilder app)
        {
            var grupo = app.MapGroup("/api/cliente");

            // --- CONSULTAS (GET) ---

            // Obtener cliente completo por ID
            grupo.MapGet("/{idCliente:int}", async (int idCliente, IClienteRepository clienteRepo) =>
            {
                var cliente = await clienteRepo.ObtenerClientePorIdAsync(idCliente);
                return cliente is not null ? Results.Ok(cliente) : Results.NotFound();
            });

            // Buscar clientes por nombre o identificación
            grupo.MapGet("/buscar/{busqueda}", async (string busqueda, IClienteRepository clienteRepo) =>
            {
                var clientes = await clienteRepo.ObtenerClientePorNombreOIdentificacionAsync(busqueda);
                return Results.Ok(clientes);
            });

            // Listar clientes
            // TODO: ObtenerClientes() en sw_cliente todavía no soporta paginado (ver Base de datos/sw_cliente_actualizaciones.sql).
            grupo.MapGet("/", async (IClienteRepository clienteRepo) =>
            {
                var clientes = await clienteRepo.ObtenerClientesAsync();
                return Results.Ok(clientes);
            });

            // Total de clientes
            grupo.MapGet("/total", async (IClienteRepository clienteRepo) =>
            {
                var total = await clienteRepo.ObtenerTotalClientesAsync();
                return Results.Ok(total);
            });

            // --- CREACIÓN (POST) ---

            // Agregar cliente base
            grupo.MapPost("/", async (NewClienteRequest cliente, IClienteRepository clienteRepo) =>
            {
                var resultado = await clienteRepo.AgregarClienteAsync(cliente);
                return resultado.IdCliente > 0
                    ? Results.Created($"/api/cliente/{resultado.IdCliente}", resultado)
                    : Results.BadRequest(new { mensaje = resultado.Mensaje });
            });

            // Agregar régimen a cliente
            grupo.MapPost("/regimen", async (NewClienteRegimenRequest regimen, IClienteRepository clienteRepo) =>
            {
                var (rows, mensaje) = await clienteRepo.AgregarClienteRegimenAsync(regimen);
                return rows > 0 ? Results.Ok(mensaje) : Results.BadRequest(mensaje);
            });

            // Agregar actividad a cliente
            grupo.MapPost("/actividad", async (NewClienteActividadRequest actividad, IClienteRepository clienteRepo) =>
            {
                var (rows, mensaje) = await clienteRepo.AgregarClienteActividadAsync(actividad);
                return rows > 0 ? Results.Ok(mensaje) : Results.BadRequest(mensaje);
            });

            // --- ACTUALIZACIÓN (PUT / PATCH) ---

            // Actualizar datos del cliente
            grupo.MapPut("/", async (UpdateClienteRequest cliente, IClienteRepository clienteRepo) =>
            {
                var (rows, mensaje) = await clienteRepo.ActualizarClienteAsync(cliente);
                return rows > 0 ? Results.Ok(mensaje) : Results.BadRequest(mensaje);
            });

            // Activar cliente (Cambio de estado puntual -> Patch)
            grupo.MapPatch("/{idCliente:int}/activar", async (int idCliente, IClienteRepository clienteRepo) =>
            {
                var (rows, mensaje) = await clienteRepo.ActivarClienteAsync(idCliente);
                return rows > 0 ? Results.Ok(mensaje) : Results.BadRequest(mensaje);
            });

            // --- ELIMINACIÓN (DELETE) ---

            // Eliminar cliente
            grupo.MapDelete("/{idCliente:int}", async (int idCliente, IClienteRepository clienteRepo) =>
            {
                var (rows, mensaje) = await clienteRepo.EliminarClienteAsync(idCliente);
                return rows > 0 ? Results.Ok(mensaje) : Results.BadRequest(mensaje);
            });
        }
    }
}
