-- Base de datos: sw_administracion
-- Motivo: se encontraron 10 consecutivos duplicados reales para el negocio 5
-- (uno usado hasta 3 veces) -- Hacienda rechaza un consecutivo repetido.
--
-- Causa: ObtenerConsecutivoPorNegocioYTipo hace un SELECT normal, sin
-- bloquear la fila, antes de que ActualizarConsecutivoPorRegistro la
-- incremente -- ambas llamadas van dentro de la misma transacción al
-- registrar una factura o una nota (ver FacturaRepository.cs /
-- NotaRepository.cs), pero sin bloqueo, dos registros casi simultáneos
-- pueden leer el mismo número antes de que el primero lo actualice.
--
-- Fix: agrega FOR UPDATE al SELECT. Dentro de una transacción, esto bloquea
-- la fila de tbl_consecutivo hasta que la transacción que la leyó termine
-- (commit o rollback) -- una segunda transacción que intente leer la misma
-- fila espera, y cuando le toca, ya ve el valor incrementado.
--
-- Aplicar con: mysql -h127.0.0.1 -uroot --default-character-set=utf8mb4 sw_administracion < este_archivo.sql

DELIMITER $$

DROP PROCEDURE IF EXISTS `ObtenerConsecutivoPorNegocioYTipo`$$

CREATE PROCEDURE `ObtenerConsecutivoPorNegocioYTipo`(`idEntidad` INT, `tipo` INT)
BEGIN
	SELECT CONCAT(c.`sucursal`, c.`punto_venta`, c.`documento_asociado`, LPAD(c.`consecutivo`,10,'0')) consecutivo
	FROM `tbl_consecutivo` c
	WHERE c.id_entidad = idEntidad AND c.`tipo` = tipo
	FOR UPDATE;
END$$

DELIMITER ;
