using SilexCore.Infrastructure.Services;

namespace SilexCore.Tests;

// NumeroALetrasHelper es lógica pura (sin dependencias) que arma el "monto en
// letras" legal de facturas/recepciones -- un error aquí produce un documento
// tributario con el monto en palabras equivocado. Se prueba exhaustivamente
// por ser barato (sin mocks) y de alto riesgo si falla en silencio.
public class NumeroALetrasHelperTests
{
    [Theory]
    [InlineData(0, "CERO COLONES")]
    [InlineData(1, "UN COLONES")]
    [InlineData(5, "CINCO COLONES")]
    [InlineData(9, "NUEVE COLONES")]
    [InlineData(10, "DIEZ COLONES")]
    [InlineData(11, "ONCE COLONES")]
    [InlineData(15, "QUINCE COLONES")]
    [InlineData(19, "DIECINUEVE COLONES")]
    [InlineData(20, "VEINTE COLONES")]
    [InlineData(21, "VEINTIUN COLONES")]
    [InlineData(29, "VEINTINUEVE COLONES")]
    [InlineData(30, "TREINTA COLONES")]
    [InlineData(42, "CUARENTA Y DOS COLONES")]
    [InlineData(99, "NOVENTA Y NUEVE COLONES")]
    [InlineData(100, "CIEN COLONES")]
    [InlineData(101, "CIENTO UN COLONES")]
    [InlineData(199, "CIENTO NOVENTA Y NUEVE COLONES")]
    [InlineData(200, "DOSCIENTOS COLONES")]
    [InlineData(500, "QUINIENTOS COLONES")]
    [InlineData(999, "NOVECIENTOS NOVENTA Y NUEVE COLONES")]
    [InlineData(1000, "MIL COLONES")]
    [InlineData(1001, "MIL UN COLONES")]
    [InlineData(2000, "DOS MIL COLONES")]
    [InlineData(21000, "VEINTIUN MIL COLONES")]
    [InlineData(56500, "CINCUENTA Y SEIS MIL QUINIENTOS COLONES")]
    [InlineData(100000, "CIEN MIL COLONES")]
    [InlineData(999999, "NOVECIENTOS NOVENTA Y NUEVE MIL NOVECIENTOS NOVENTA Y NUEVE COLONES")]
    [InlineData(1000000, "UN MILLON COLONES")]
    [InlineData(1000001, "UN MILLON UN COLONES")]
    [InlineData(2000000, "DOS MILLONES COLONES")]
    [InlineData(2500000, "DOS MILLONES QUINIENTOS MIL COLONES")]
    public void Convertir_EnterosSinCentavos_DevuelveElTextoEsperado(long monto, string esperado)
    {
        Assert.Equal(esperado, NumeroALetrasHelper.Convertir(monto));
    }

    [Theory]
    [InlineData("56500.50", "CINCUENTA Y SEIS MIL QUINIENTOS COLONES CON 50/100")]
    [InlineData("0.01", "CERO COLONES CON 01/100")]
    [InlineData("0.99", "CERO COLONES CON 99/100")]
    [InlineData("1.005", "UN COLONES CON 01/100")] // redondeo hacia arriba (AwayFromZero)
    public void Convertir_ConCentavos_LosMuestraComoFraccionDe100(string monto, string esperado)
    {
        Assert.Equal(esperado, NumeroALetrasHelper.Convertir(decimal.Parse(monto, System.Globalization.CultureInfo.InvariantCulture)));
    }

    [Fact]
    public void Convertir_SinCentavos_NoAgregaElSufijoCon100()
    {
        var resultado = NumeroALetrasHelper.Convertir(1500.00m);
        Assert.DoesNotContain("CON", resultado);
    }

    [Fact]
    public void Convertir_PermiteCambiarElNombreDeLaMoneda()
    {
        Assert.Equal("CIEN DOLARES", NumeroALetrasHelper.Convertir(100m, "DOLARES"));
    }

    [Fact]
    public void Convertir_MontoNegativo_UsaElValorAbsolutoSinIndicarElSigno()
    {
        // Documenta el comportamiento actual: el signo se descarta por completo.
        // Si algún flujo (p.ej. notas de crédito) necesita reflejar montos
        // negativos en el texto legal, este test debe actualizarse a la vez
        // que se decida cómo representarlo.
        Assert.Equal(NumeroALetrasHelper.Convertir(1500m), NumeroALetrasHelper.Convertir(-1500m));
    }
}
