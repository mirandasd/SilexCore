using System.Globalization;
using System.Text;

namespace SilexCore.Infrastructure.Services;

// Convierte un monto a su representación en letras en español, para el "Monto en letras"
// de la factura/recepción (ej. 56500.00 -> "CINCUENTA Y SEIS MIL QUINIENTOS COLONES").
public static class NumeroALetrasHelper
{
    private static readonly string[] Unidades =
        ["", "UN", "DOS", "TRES", "CUATRO", "CINCO", "SEIS", "SIETE", "OCHO", "NUEVE"];

    private static readonly string[] Decenas =
        ["", "DIEZ", "VEINTE", "TREINTA", "CUARENTA", "CINCUENTA", "SESENTA", "SETENTA", "OCHENTA", "NOVENTA"];

    private static readonly string[] Especiales =
    {
        "DIEZ", "ONCE", "DOCE", "TRECE", "CATORCE", "QUINCE", "DIECISEIS", "DIECISIETE", "DIECIOCHO", "DIECINUEVE"
    };

    private static readonly string[] Centenas =
    {
        "", "CIENTO", "DOSCIENTOS", "TRESCIENTOS", "CUATROCIENTOS", "QUINIENTOS",
        "SEISCIENTOS", "SETECIENTOS", "OCHOCIENTOS", "NOVECIENTOS"
    };

    public static string Convertir(decimal monto, string nombreMoneda = "COLONES", string nombreCentavo = "CÉNTIMOS")
    {
        var entero = (long)Math.Truncate(Math.Abs(monto));
        var centavos = (int)Math.Round((Math.Abs(monto) - entero) * 100, MidpointRounding.AwayFromZero);

        var texto = entero == 0 ? "CERO" : ConvertirEntero(entero);
        var resultado = $"{texto} {nombreMoneda}";

        if (centavos > 0)
            resultado += $" CON {centavos:00}/100";

        return resultado.Trim();
    }

    private static string ConvertirEntero(long numero)
    {
        if (numero == 0) return "";
        if (numero < 10) return Unidades[numero];
        if (numero < 20) return numero == 10 ? "DIEZ" : Especiales[numero - 10];
        if (numero < 100) return ConvertirDecenas(numero);
        if (numero < 1000) return ConvertirCentenas(numero);
        if (numero < 1_000_000) return ConvertirMiles(numero);
        if (numero < 1_000_000_000_000) return ConvertirMillones(numero);

        return numero.ToString(CultureInfo.InvariantCulture);
    }

    private static string ConvertirDecenas(long numero)
    {
        var decena = numero / 10;
        var unidad = numero % 10;

        if (decena == 2 && unidad > 0) return $"VEINTI{Unidades[unidad]}";
        if (unidad == 0) return Decenas[decena];
        return $"{Decenas[decena]} Y {Unidades[unidad]}";
    }

    private static string ConvertirCentenas(long numero)
    {
        if (numero == 100) return "CIEN";

        var centena = numero / 100;
        var resto = numero % 100;
        var texto = Centenas[centena];

        return resto == 0 ? texto : $"{texto} {ConvertirEntero(resto)}";
    }

    private static string ConvertirMiles(long numero)
    {
        var miles = numero / 1000;
        var resto = numero % 1000;

        var prefijoMiles = miles == 1 ? "MIL" : $"{ConvertirEntero(miles)} MIL";
        return resto == 0 ? prefijoMiles : $"{prefijoMiles} {ConvertirEntero(resto)}";
    }

    private static string ConvertirMillones(long numero)
    {
        var millones = numero / 1_000_000;
        var resto = numero % 1_000_000;

        var prefijoMillones = millones == 1 ? "UN MILLON" : $"{ConvertirEntero(millones)} MILLONES";
        return resto == 0 ? prefijoMillones : $"{prefijoMillones} {ConvertirEntero(resto)}";
    }
}
