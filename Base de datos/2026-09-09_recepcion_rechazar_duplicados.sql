-- Base de datos: sw_procesos
-- Motivo: AgregarRecepcionDocumento insertaba sin revisar si esa clave_documento
-- ya estaba registrada para el mismo negocio. La única protección era el chequeo
-- "validar-clave" del frontend antes de mostrar el formulario -- puramente de UX,
-- no lo hacía cumplir la base de datos ni el SP. Se confirmó en producción: la
-- clave 50625032600310124129300100016010000000745168477317 quedó registrada 4
-- veces, las 4 para el mismo negocio (id_entidad 5).
--
-- Aplicar con: mysql -h127.0.0.1 -uroot --default-character-set=utf8mb4 sw_procesos < este_archivo.sql

DELIMITER $$

DROP PROCEDURE IF EXISTS `AgregarRecepcionDocumento`$$

CREATE PROCEDURE `AgregarRecepcionDocumento`(IN `idEntidad` INT, IN `nombreEmisor` VARCHAR(100), IN `tipoIdentificacion` VARCHAR(3), IN `identificacion` VARCHAR(150), IN `idTipoComprobante` INT, IN `clave` VARCHAR(50), IN `impuesto` DECIMAL(18,5), IN `impuestoAcreditar` DECIMAL(18,5), IN `gastoAplicable` DECIMAL(18,5), IN `total` DECIMAL(18,5), IN `idMoneda` INT, IN `monedaValor` DECIMAL(10,2), IN `detalle` VARCHAR(80), IN `idEstadoRecepcion` INT, IN `idCondicionImpuesto` INT, OUT `msj` VARCHAR(100))
BEGIN
	DECLARE IdTipoIdentificacion INT DEFAULT 0;
	DECLARE IdCliente INT DEFAULT 0;
	DECLARE IdExistente INT DEFAULT 0;

	SET IdExistente = (SELECT rd.id_recepcion_documento FROM tbl_recepcion_documento rd
		WHERE rd.id_entidad = idEntidad AND rd.clave_documento = clave LIMIT 1);

	IF IdExistente IS NOT NULL THEN
		SET msj = CONCAT('Este documento ya fue registrado antes (recepción #', IdExistente, ').');
	ELSE
		SET IdCliente = (SELECT c.`IdCliente` FROM sw_cliente.`cliente` c WHERE REPLACE(c.Identificacion, '-', '') = identificacion LIMIT 1);

		IF IdCliente IS NULL THEN
			SET IdTipoIdentificacion = (SELECT ti.`id` FROM sw_hacienda.`tipoidentificacion` ti WHERE ti.`codigo` = tipoIdentificacion);
			INSERT INTO sw_cliente.`cliente`(`IdTipoIdentificacion`, `Identificacion`, `Nombre`)
			VALUES (IdTipoIdentificacion, identificacion, nombreEmisor);
			SET IdCliente = LAST_INSERT_ID();
		END IF;

		INSERT INTO `tbl_recepcion_documento`(`id_entidad`, `id_cliente`, `id_tipo_comprobante`, `clave_documento`, `impuesto`, `impuesto_acreditar`, `gasto_aplicable`, `total`, `id_moneda`, `moneda_valor`, `detalle`, `estado_recepcion`, `id_condicion_impuesto`)
		VALUES (idEntidad, IdCliente, idTipoComprobante, clave, impuesto, impuestoAcreditar, gastoAplicable, total, idMoneda, monedaValor, detalle, idEstadoRecepcion, idCondicionImpuesto);
		SET msj = LAST_INSERT_ID();
		SELECT IdCliente;
	END IF;
END$$

DELIMITER ;
