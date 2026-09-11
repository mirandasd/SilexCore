-- AgregarUsuario rechazaba SIEMPRE el registro de un usuario nuevo (con cualquier
-- correo) en cuanto ya existiera al menos una fila en tbl_usuario_perfil.
-- Causa: el parámetro de entrada se llamaba `Email` y la columna se llama `email`;
-- MySQL resuelve nombres de columna/parámetro sin distinguir mayúsculas/minúsculas,
-- así que `WHERE email = Email` se interpretaba como `email = email` (columna
-- contra sí misma), que es verdadero para cualquier fila existente -- el EXISTS()
-- nunca comparaba de verdad contra el correo recibido.
-- Encontrado al intentar registrar un segundo usuario (rol Cajero) para probar
-- los permisos por rol: nadie había podido registrar un usuario nuevo desde que
-- existe el primero (Diego).
-- Fix: renombrar el parámetro para que no choque con el nombre de la columna.

DROP PROCEDURE IF EXISTS `AgregarUsuario`;

DELIMITER $$

CREATE PROCEDURE `AgregarUsuario`(
    `AuthUserId` VARCHAR(64),
    `IdEntidad` INT,
    `NombrePersona` VARCHAR(150),
    `pEmail` VARCHAR(150),
    `Rol` VARCHAR(50),
    `Telefono` VARCHAR(20),
    `Cargo` VARCHAR(100),
    OUT `msj` VARCHAR(100)
)
BEGIN
    IF EXISTS(SELECT * FROM `tbl_usuario_perfil` WHERE `email` = pEmail) THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Ya existe un perfil con ese correo';
    END IF;

    INSERT INTO `tbl_usuario_perfil`(`auth_user_id`, `id_entidad`, `nombre`, `email`, `rol`, `telefono`, `cargo`)
    VALUES (AuthUserId, IdEntidad, NombrePersona, pEmail, Rol, Telefono, Cargo);

    SET msj = 'Usuario registrado correctamente';
END$$

DELIMITER ;
