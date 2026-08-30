using System.Security.Claims;
using System.Text.Json;
using SilexCore.Api.Interfaces;
using SilexCore.Domain.Dtos;
using SilexCore.Domain.Interfaces.Infraestructura.Persistence;
using SilexCore.Domain.Interfaces.Infraestructura.Services;

namespace SilexCore.Api.Endpoints.v1;

public class UsuarioEndpoints : IEndpointModule
{
    public void RegistrarEndpoints(IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup("/api/usuario")
                       .WithTags("Usuarios")
                       .RequireAuthorization();

        // Listar usuarios: solo administradores (mantenimiento de usuarios)
        grupo.MapGet("/", async (IUsuarioRepository usuarioRepo) =>
        {
            var usuarios = await usuarioRepo.ListarUsuariosAsync();
            return Results.Ok(usuarios);
        })
        .RequireAuthorization("RequiereRolAdmin")
        .WithName("ListarUsuarios");

        // Negocios disponibles para asignar al crear un usuario
        grupo.MapGet("/negocios", async (IUsuarioRepository usuarioRepo) =>
        {
            var negocios = await usuarioRepo.ListarNegociosAsync();
            return Results.Ok(negocios);
        })
        .RequireAuthorization("RequiereRolAdmin")
        .WithName("ListarNegociosParaUsuario");

        // --- REGISTRO (flujo: identidad en AuthService + perfil local en SilexCore) ---

        grupo.MapPost("/registro", async (RegistrarUsuarioRequest usuario, IAutenticacionService autenticacion, IUsuarioRepository usuarioRepo, IBitacoraService bitacora) =>
        {
            var payload = JsonSerializer.Serialize(usuario);

            // 1. Crear la identidad (credenciales) en AuthService
            var (identidadCreada, mensajeIdentidad, authUserId) = await autenticacion.RegistrarIdentidadAsync(usuario);
            if (!identidadCreada || authUserId is null)
            {
                await bitacora.RegistrarErrorBitacoraAsync($"Error al registrar identidad para {usuario.Email}: {mensajeIdentidad}", payload);
                return Results.BadRequest(new { mensaje = mensajeIdentidad });
            }

            // 2. Persistir el perfil local (negocio, cargo, teléfono, etc.)
            var (rows, mensajePerfil) = await usuarioRepo.AgregarUsuarioAsync(authUserId, usuario);
            if (rows <= 0)
            {
                await bitacora.RegistrarErrorBitacoraAsync($"Identidad creada pero falló el perfil local para {usuario.Email}: {mensajePerfil}", payload);
                return Results.Ok(new { mensaje = "Usuario creado en el servicio de autenticación, pero falló el registro del perfil local.", detalle = mensajePerfil });
            }

            await bitacora.RegistrarNegocioBitacoraAsync($"Usuario registrado: {usuario.Email}", payload);
            return Results.Created("/api/usuario/registro", new { mensaje = "Usuario registrado correctamente. Se enviaron las credenciales temporales por correo." });
        })
        .RequireAuthorization("RequiereRolAdmin")
        .WithName("RegistrarUsuario");

        // --- PERFIL ---

        // Mi propio perfil (cualquier usuario autenticado, sin importar el rol)
        grupo.MapGet("/perfil/me", async (ClaimsPrincipal usuario, IUsuarioRepository usuarioRepo) =>
        {
            var email = usuario.FindFirst(ClaimTypes.Email)?.Value;
            if (email is null) return Results.Unauthorized();

            var perfil = await usuarioRepo.ObtenerPerfilPorEmailAsync(email);
            return perfil is not null ? Results.Ok(perfil) : Results.NotFound();
        })
        .WithName("ObtenerMiPerfil");

        // Obtener perfil de cualquier usuario por id: solo administradores
        grupo.MapGet("/perfil/{idUsuario:int}", async (int idUsuario, IUsuarioRepository usuarioRepo) =>
        {
            var perfil = await usuarioRepo.ObtenerPerfilPorIdAsync(idUsuario);
            return perfil is not null ? Results.Ok(perfil) : Results.NotFound();
        })
        .RequireAuthorization("RequiereRolAdmin")
        .WithName("ObtenerPerfilPorId");

        // Obtener perfil de cualquier usuario por correo: solo administradores
        grupo.MapGet("/perfil/email/{email}", async (string email, IUsuarioRepository usuarioRepo) =>
        {
            var perfil = await usuarioRepo.ObtenerPerfilPorEmailAsync(email);
            return perfil is not null ? Results.Ok(perfil) : Results.NotFound();
        })
        .RequireAuthorization("RequiereRolAdmin")
        .WithName("ObtenerPerfilPorEmail");

        // Actualizar perfil (nombre, teléfono, cargo, foto): un usuario normal solo
        // puede tocar el suyo; un administrador puede actualizar el de cualquiera.
        grupo.MapPut("/perfil", async (ActualizarPerfilRequest perfil, ClaimsPrincipal usuarioActual, IUsuarioRepository usuarioRepo, IBitacoraService bitacora) =>
        {
            var esAdmin = usuarioActual.IsInRole("Administrador");
            if (!esAdmin)
            {
                var email = usuarioActual.FindFirst(ClaimTypes.Email)?.Value;
                var propio = email is not null ? await usuarioRepo.ObtenerPerfilPorEmailAsync(email) : null;
                if (propio is null || propio.IdUsuario != perfil.IdUsuario)
                    return Results.Forbid();
            }

            var (rows, mensaje) = await usuarioRepo.ActualizarPerfilAsync(perfil);
            var payload = JsonSerializer.Serialize(perfil);

            if (rows <= 0)
            {
                await bitacora.RegistrarErrorBitacoraAsync($"Error al actualizar perfil {perfil.IdUsuario}: {mensaje}", payload);
                return Results.BadRequest(new { mensaje });
            }

            await bitacora.RegistrarNegocioBitacoraAsync($"Perfil actualizado: {perfil.IdUsuario}", payload);
            return Results.Ok(new { mensaje });
        })
        .WithName("ActualizarPerfil");
    }
}
