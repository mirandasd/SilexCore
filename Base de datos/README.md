# Base de datos

Los stored procedures de SilexCore viven solo en la base real (MariaDB/XAMPP) —
no había ningún registro versionado de sus cambios. Esta carpeta guarda, de
ahora en adelante, cada cambio de SP/tabla como un script reaplicable.

## Convención

- Un archivo por cambio: `AAAA-MM-DD_descripcion-corta.sql`.
- Cada script debe poder ejecutarse de nuevo sin romper nada (`DROP PROCEDURE IF EXISTS`
  antes de `CREATE PROCEDURE`, `ADD COLUMN IF NOT EXISTS` cuando aplique, etc.).
- Encabezado corto explicando qué cambia y por qué (el motivo casi nunca es obvio
  leyendo el SP a secas).
- Aplicar siempre con el cliente de MySQL en `utf8mb4` explícito -- sin eso, los
  acentos se corrompen al guardarse (ya pasó una vez, ver historial del proyecto):

  ```
  mysql -h127.0.0.1 -uroot --default-character-set=utf8mb4 <base_de_datos> < script.sql
  ```

## Scripts

| Archivo | Base de datos | Qué hace |
|---|---|---|
| `2026-09-09_recepcion_rechazar_duplicados.sql` | `sw_procesos` | `AgregarRecepcionDocumento` rechaza una recepción si ya existe la misma clave para el mismo negocio, en vez de insertarla de nuevo. |
| `2026-09-10_certificados_por_negocio_activo.sql` | `sw_administracion` | Nuevo SP `ObtenerCertificadosPorNegociosActivos`: lista los negocios activos junto con su certificado (si tienen), para el estado de vencimiento en el index de silex-app. |
