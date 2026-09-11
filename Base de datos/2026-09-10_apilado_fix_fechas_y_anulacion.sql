-- Bases de datos: sw_procesos
-- Motivo: al conectar el balance de apilado (producción vs venta de
-- materiales) al home de silex-app se encontraron 2 bugs reales:
--
-- 1. ObtenerBalanceApiladoPorNegocio compara "fecha BETWEEN FechaInicio AND
--    FechaFin" con FechaFin de tipo DATE -- MySQL lo trata como
--    "FechaFin 00:00:00", así que cualquier venta o producción del día de
--    HOY (después de medianoche) queda fuera del balance. Confirmado en vivo:
--    una factura registrada a las 20:04 no aparecía pidiendo el rango hasta
--    hoy, sí aparecía pidiendo hasta mañana.
--
-- 2. Registrar una nota de crédito que "Anula documento de referencia" nunca
--    revertía la venta: AgregarNotaCredito solo insertaba en tbl_nota, nunca
--    tocaba tbl_factura_detalle.estado (que el balance sí filtra, pero como
--    nada lo ponía en 0, el filtro no hacía nada). Tampoco había forma de
--    saber que una factura ya fue anulada -- se le podía volver a hacer otra
--    nota. Se agrega tbl_factura.estado (1 = vigente, 0 = anulada): al anular
--    se pone en 0 la factura y sus líneas, y la búsqueda de factura para
--    hacer una nota nueva ya no la muestra.
--
-- Aplicar con: mysql -h127.0.0.1 -uroot --default-character-set=utf8mb4 sw_procesos < este_archivo.sql

ALTER TABLE `tbl_factura` ADD COLUMN IF NOT EXISTS `estado` TINYINT(1) NOT NULL DEFAULT 1 AFTER `CodigoActividad`;

DELIMITER $$

DROP PROCEDURE IF EXISTS `ObtenerBalanceApiladoPorNegocio`$$

CREATE PROCEDURE `ObtenerBalanceApiladoPorNegocio`(`IdEntidad` INT, `FechaInicio` DATE, `FechaFin` DATE)
BEGIN
    SELECT
        m.material Material,
        IFNULL(p.total, 0) TotalProducido,
        IFNULL(v.total, 0) TotalVendido,
        IFNULL(p.total, 0) - IFNULL(v.total, 0) Balance
    FROM (
        SELECT 'piedra_cuarta' material UNION ALL
        SELECT 'polvo_piedra' UNION ALL
        SELECT 'arena' UNION ALL
        SELECT 'lastre'
    ) m
    LEFT JOIN (
        SELECT 'piedra_cuarta' material, SUM(piedra_cuarta) total FROM tbl_material_produccion WHERE id_entidad = IdEntidad AND fecha >= FechaInicio AND fecha < FechaFin + INTERVAL 1 DAY
        UNION ALL
        SELECT 'polvo_piedra', SUM(polvo_piedra) FROM tbl_material_produccion WHERE id_entidad = IdEntidad AND fecha >= FechaInicio AND fecha < FechaFin + INTERVAL 1 DAY
        UNION ALL
        SELECT 'arena', SUM(arena) FROM tbl_material_produccion WHERE id_entidad = IdEntidad AND fecha >= FechaInicio AND fecha < FechaFin + INTERVAL 1 DAY
        UNION ALL
        SELECT 'lastre', SUM(lastre) FROM tbl_material_produccion WHERE id_entidad = IdEntidad AND fecha >= FechaInicio AND fecha < FechaFin + INTERVAL 1 DAY
    ) p ON p.material = m.material
    LEFT JOIN (
        SELECT amo.material, SUM(fd.cantidad) total
        FROM tbl_factura_detalle fd
        INNER JOIN tbl_factura f ON fd.id_factura = f.id_factura
        INNER JOIN tbl_apilado_material_opcion amo ON fd.id_opcion_venta = amo.id_opcion_venta
        WHERE f.id_entidad = IdEntidad AND f.fecha >= FechaInicio AND f.fecha < FechaFin + INTERVAL 1 DAY AND fd.estado = 1
        GROUP BY amo.material
    ) v ON v.material = m.material;
END$$

DROP PROCEDURE IF EXISTS `AgregarNotaCredito`$$

CREATE PROCEDURE `AgregarNotaCredito`(`idFactura` INT, `idAccion` INT, `consecutivo` VARCHAR(20), `detalle` VARCHAR(180), OUT `msj` VARCHAR(100))
BEGIN
	INSERT INTO `tbl_nota`
    (
            `id_factura`, `tipo_nota`, `consecutivo`, `id_accion`, `detalle`
    )
    VALUES
    (idFactura, 3, consecutivo, idAccion, detalle);
    SET msj = LAST_INSERT_ID();

    -- idAccion 1 = "Anular documento de referencia": la venta ya no existe,
    -- se revierte para el balance de apilado y para que no se pueda volver a
    -- anular la misma factura.
    IF idAccion = 1 THEN
        UPDATE `tbl_factura` SET `estado` = 0 WHERE `id_factura` = idFactura;
        UPDATE `tbl_factura_detalle` SET `estado` = 0 WHERE `id_factura` = idFactura;
    END IF;
END$$

DROP PROCEDURE IF EXISTS `ObtenerFacturaPorConsecutivoParaNota`$$

CREATE PROCEDURE `ObtenerFacturaPorConsecutivoParaNota`(`IdEntidad` INT, `Consecutivo` VARCHAR(20))
BEGIN
	SELECT
		f.id_factura id,
		f.Consecutivo consecutivo,
		ff.Clave clave,
		c.Nombre cliente,
		DATE_FORMAT(f.fecha, '%d/%m/%Y') fecha,
		IFNULL(SUM(fd.cantidad * fd.precio * (1 + IFNULL(tt.valor, 0) / 100)), 0) monto,
		IF(ff.EstadoHacienda = 1, 'Aceptada', IF(ff.EstadoHacienda = 3, 'Rechazada', 'Pendiente')) estado
	FROM `tbl_factura` f
	LEFT JOIN sw_cliente.cliente c ON f.id_cliente = c.IdCliente
	LEFT JOIN facturacionfacturahst ff ON f.id_factura = ff.IdFactura AND ff.Estado = 1
	LEFT JOIN tbl_factura_detalle fd ON f.id_factura = fd.id_factura AND fd.estado = 1
	LEFT JOIN sw_hacienda.tipotarifa tt ON fd.id_tipo_tarifa = tt.id
	WHERE f.id_entidad = IdEntidad AND f.Consecutivo LIKE CONCAT('%', Consecutivo) AND f.estado = 1
	GROUP BY f.id_factura
	ORDER BY f.fecha DESC
	LIMIT 8;
END$$

DELIMITER ;
