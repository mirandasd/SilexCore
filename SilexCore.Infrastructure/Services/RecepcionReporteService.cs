using System.Globalization;
using SilexCore.Domain.Interfaces.Infraestructura.Persistence;
using SilexCore.Domain.Interfaces.Infraestructura.Services;

namespace SilexCore.Infrastructure.Services;

public class RecepcionReporteService(
    IRecepcionRepository recepcionRepo,
    IUsuarioRepository usuarioRepo,
    IImagenReporteService imagenes,
    IReporteService reportes) : IRecepcionReporteService
{
    public async Task<(byte[] Contenido, string ContentType)?> GenerarReporteAsync(int idRecepcionDocumento, string formato = "pdf")
    {
        var recepcion = await recepcionRepo.ObtenerPorIdAsync(idRecepcionDocumento);
        if (recepcion is null) return null;

        var entidad = await usuarioRepo.ObtenerEntidadReporteAsync(recepcion.IdEntidad);
        var configuracion = await usuarioRepo.ObtenerConfiguracionDocumentoAsync();

        var logoDataUri = await imagenes.ObtenerComoDataUriAsync(entidad?.LogoArchivo);
        var fondoDataUri = await imagenes.ObtenerComoDataUriAsync(entidad?.FondoArchivo);

        var datos = new
        {
            negocio = new
            {
                nombre_comercial = entidad?.NombreComercial,
                razon_social = entidad?.RazonSocial,
                identificacion = entidad?.Identificacion,
                codigo_pais = entidad?.CodigoPais,
                telefono = entidad?.Telefono,
                correo = entidad?.Correo,
                direccion_completa = $"{entidad?.Provincia}, {entidad?.Canton}, {entidad?.Distrito}, {entidad?.Direccion}",
                logo_data_uri = logoDataUri,
                fondo_data_uri = fondoDataUri
            },
            emisor = new
            {
                identificacion = recepcion.IdentificacionEmisor,
                nombre = recepcion.NombreEmisor,
                telefono = recepcion.TelefonoEmisor,
                correo = recepcion.CorreoEmisor
            },
            documento = new
            {
                consecutivo = recepcion.Consecutivo,
                clave = recepcion.Clave,
                fecha = recepcion.Fecha,
                estado_recepcion = recepcion.EstadoRecepcion,
                tipo_comprobante = recepcion.TipoComprobante,
                detalle = recepcion.Detalle
            },
            totales = new
            {
                impuesto = Formato(recepcion.Impuesto),
                impuesto_acreditar = Formato(recepcion.ImpuestoAcreditar),
                gasto_aplicable = Formato(recepcion.GastoAplicable),
                total = Formato(recepcion.Total)
            },
            monto_en_letras = NumeroALetrasHelper.Convertir(recepcion.Total, (recepcion.Moneda ?? "Colones").ToUpperInvariant()),
            configuracion = new
            {
                version = configuracion?.VersionComprobante ?? "4.4",
                texto_autorizacion = configuracion?.TextoAutorizacion
            },
            pagina = 1
        };

        return await reportes.GenerarReporteAsync("Recepcion", datos, formato);
    }

    private static string Formato(decimal valor) => valor.ToString("N2", CultureInfo.InvariantCulture);
}
