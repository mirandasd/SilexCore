-- ObtenerPerfilUsuarioPorEmail devolvía siempre el primer usuario de la tabla
-- (Diego), sin importar el correo con el que se había iniciado sesión.
-- Mismo bug que AgregarUsuario (ver 2026-09-10_agregar_usuario_fix_colision_email.sql):
-- el parámetro se llamaba `Email` y la columna `email`; MySQL resuelve nombres de
-- columna/parámetro sin distinguir mayúsculas/minúsculas, así que
-- `WHERE email = Email` se interpretaba como `email = email` (siempre verdadero),
-- y como no hay LIMIT 1, Dapper (QueryFirstOrDefaultAsync) se quedaba con la
-- primera fila del resultado completo de la tabla.
-- Encontrado al loguear al usuario de prueba con rol Cajero: el token JWT traía
-- el rol y correo correctos, pero el "perfil" devuelto por /api/auth/login era
-- el de Diego (Administrador). Esto afecta a CUALQUIER usuario que no sea el
-- primero registrado -- probablemente pasaba desapercibido porque hasta ahora
-- solo existía un usuario real en el sistema.
-- Fix: renombrar el parámetro para que no choque con el nombre de la columna.

DROP PROCEDURE IF EXISTS `ObtenerPerfilUsuarioPorEmail`;

DELIMITER $$

CREATE PROCEDURE `ObtenerPerfilUsuarioPorEmail`(`pEmail` VARCHAR(150))
BEGIN
    SELECT `id_usuario` IdUsuario, `id_entidad` IdEntidad, `auth_user_id` AuthUserId, `nombre` Nombre,
        `email` Email, `telefono` Telefono, `cargo` Cargo, `foto_url` FotoUrl, `rol` Rol, `estado` Activo
    FROM `tbl_usuario_perfil`
    WHERE `email` = pEmail;
END$$

DELIMITER ;
