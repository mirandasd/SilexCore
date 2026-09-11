-- Base de datos: sw_administracion
-- Motivo: nueva funcionalidad -- estado de vencimiento de certificados de firma,
-- mostrado en el index de silex-app. InvoicingService.Api necesita listar todos
-- los negocios activos junto con su certificado (si tienen) para leer la fecha
-- real de vencimiento desde el propio archivo .p12.
--
-- Aplicar con: mysql -h127.0.0.1 -uroot --default-character-set=utf8mb4 sw_administracion < este_archivo.sql

DELIMITER $$

DROP PROCEDURE IF EXISTS `ObtenerCertificadosPorNegociosActivos`$$

CREATE PROCEDURE `ObtenerCertificadosPorNegociosActivos`()
BEGIN
	SELECT
		e.id_entidad IdEntidad,
		e.nombre_comercial NombreComercial,
		c.id_certificado IdCertificado,
		c.certificado Certificado,
		c.pass Pass,
		c.ruta Ruta
	FROM tbl_entidad e
	LEFT JOIN tbl_certificado c ON c.id_entidad = e.id_entidad
	WHERE e.estado = 1
	ORDER BY e.nombre_comercial;
END$$

DELIMITER ;
