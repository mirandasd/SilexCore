using System.Net;
using Refit;

namespace SilexCore.Tests.TestHelpers;

// Refit.ApiResponse<T> exige que el HttpResponseMessage tenga un RequestMessage
// asociado (si no, lanza ArgumentException al construirlo) -- este helper evita
// repetir ese detalle en cada mock de un cliente Refit.
internal static class RefitResponseFactory
{
    private static HttpResponseMessage Http(HttpStatusCode codigo, string urlFicticia) => new(codigo)
    {
        RequestMessage = new HttpRequestMessage(HttpMethod.Post, urlFicticia)
    };

    public static ApiResponse<T> Ok<T>(T contenido, string urlFicticia = "http://localhost/api/prueba") =>
        new(Http(HttpStatusCode.OK, urlFicticia), contenido, new RefitSettings());

    public static ApiResponse<T> Fallo<T>(HttpStatusCode codigo = HttpStatusCode.BadRequest, string urlFicticia = "http://localhost/api/prueba") =>
        new(Http(codigo, urlFicticia), default, new RefitSettings());
}
