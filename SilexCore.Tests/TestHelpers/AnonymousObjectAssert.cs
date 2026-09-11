namespace SilexCore.Tests.TestHelpers;

// Los repositorios de SilexCore arman los parámetros de cada stored procedure
// como objetos anónimos (`new { IdCliente = idCliente }`). Para comprobar en
// las pruebas que el nombre y el valor exactos llegan tal cual al SP -- la
// clase de bug que ya rompió AgregarUsuario y ObtenerPerfilUsuarioPorEmail --
// hace falta leerlos por reflexión, porque el tipo anónimo no es accesible
// desde el proyecto de pruebas.
internal static class AnonymousObjectAssert
{
    public static object? Prop(object? parametros, string nombre)
    {
        if (parametros is null)
            throw new InvalidOperationException("Los parámetros enviados al SP son null.");

        var propiedad = parametros.GetType().GetProperty(nombre)
            ?? throw new InvalidOperationException(
                $"No se encontró la propiedad '{nombre}' entre los parámetros enviados: " +
                string.Join(", ", parametros.GetType().GetProperties().Select(p => p.Name)));

        return propiedad.GetValue(parametros);
    }

    public static bool TieneProp(object? parametros, string nombre) =>
        parametros?.GetType().GetProperty(nombre) is not null;
}
