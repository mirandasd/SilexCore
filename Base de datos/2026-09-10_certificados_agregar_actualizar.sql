-- Base de datos: sw_administracion
-- Motivo: mantenimiento de certificados desde la interfaz -- antes solo se
-- podían registrar a mano en la base. Get-or-update por id_entidad: cada
-- negocio tiene a lo sumo un certificado activo (igual que hoy), así que
-- subir uno nuevo reemplaza el registro existente en vez de duplicarlo.
--
-- Aplicar con: mysql -h127.0.0.1 -uroot --default-character-set=utf8mb4 sw_administracion < este_archivo.sql

DELIMITER $$

DROP PROCEDURE IF EXISTS `AgregarActualizarCertificado`$$

CREATE PROCEDURE `AgregarActualizarCertificado`(
	IN `idEntidad` INT,
	IN `p_certificado` VARCHAR(150),
	IN `p_pass` VARCHAR(100),
	IN `p_ruta` VARCHAR(250),
	OUT `msj` VARCHAR(100)
)
BEGIN
	DECLARE idExistente INT DEFAULT NULL;

	SET idExistente = (SELECT id_certificado FROM tbl_certificado WHERE id_entidad = idEntidad LIMIT 1);

	IF idExistente IS NOT NULL THEN
		UPDATE tbl_certificado
		SET certificado = p_certificado, pass = p_pass, ruta = p_ruta
		WHERE id_certificado = idExistente;
		SET msj = idExistente;
	ELSE
		INSERT INTO tbl_certificado (id_entidad, certificado, pass, ruta)
		VALUES (idEntidad, p_certificado, p_pass, p_ruta);
		SET msj = LAST_INSERT_ID();
	END IF;
END$$

DELIMITER ;
