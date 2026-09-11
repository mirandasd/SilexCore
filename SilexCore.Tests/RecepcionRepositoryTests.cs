using System.Data;
using Karin.Persistence.Dapper.Interfaces;
using Moq;
using SilexCore.Domain.Dtos;
using SilexCore.Domain.Interfaces.Infraestructura.Persistence;
using SilexCore.Infrastructure.Persistence;
using SilexCore.Tests.TestHelpers;

namespace SilexCore.Tests;

public class RecepcionRepositoryTests
{
    private readonly Mock<ISqlExecutor> _sqlExecutor = new();
    private readonly IRecepcionRepository _repo;

    public RecepcionRepositoryTests()
    {
        _repo = new RecepcionRepository(_sqlExecutor.Object);
    }

    [Fact]
    public async Task ValidarClaveRecepcionDeDocumentoAsync_EnviaLaClave()
    {
        await _repo.ValidarClaveRecepcionDeDocumentoAsync("50601...clave...");

        _sqlExecutor.Verify(s => s.QueryFirstOrDefaultAsync<int?>("ValidarClaveRecepcionDeDocumento",
            It.Is<object>(p => (string)AnonymousObjectAssert.Prop(p, "clave")! == "50601...clave..."),
            It.IsAny<string>(), It.IsAny<CommandType>()), Times.Once);
    }

    [Fact]
    public async Task AgregarRecepcionAsync_LlamaAgregarRecepcionDocumentoConElRequestTalCual()
    {
        var recepcion = new RecepcionRequest { IdEntidad = 1, Clave = "clave-1" };
        _sqlExecutor.Setup(s => s.ExecuteSpWithOutputAsync("AgregarRecepcionDocumento", recepcion, "msj", It.IsAny<string>()))
            .ReturnsAsync((1, "ok"));

        await _repo.AgregarRecepcionAsync(recepcion);

        _sqlExecutor.Verify(s => s.ExecuteSpWithOutputAsync("AgregarRecepcionDocumento", recepcion, "msj", It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task ObtenerPorIdAsync_EnviaIdRecepcionDocumento()
    {
        await _repo.ObtenerPorIdAsync(7);

        _sqlExecutor.Verify(s => s.QueryFirstOrDefaultAsync<RecepcionDetalleResponse?>("ObtenerRecepcionDocumentoPorId",
            It.Is<object>(p => (int)AnonymousObjectAssert.Prop(p, "IdRecepcionDocumento")! == 7),
            It.IsAny<string>(), It.IsAny<CommandType>()), Times.Once);
    }

    [Fact]
    public async Task GuardarHistoricoRecepcionAsync_EnviaConsecutivoNoIdComoParametroDeReferencia()
    {
        // A diferencia de Factura/Nota (que guardan su propio id como idDocumento),
        // el histórico de Recepción identifica el documento por Consecutivo, no por
        // idRecepcionDocumento -- así lo espera SP_AgregarHistoricoRecepcion.
        await _repo.GuardarHistoricoRecepcionAsync(
            idRecepcion: 3, consecutivo: "00100001010000000001", pathXml: "x.xml", pathPdf: "x.pdf", pathRespuesta: "r.xml", estadoEnvio: 1, estadoHacienda: 2);

        _sqlExecutor.Verify(s => s.ExecuteSpWithOutputAsync("SP_AgregarHistoricoRecepcion",
            It.Is<object>(p =>
                (int)AnonymousObjectAssert.Prop(p, "idDocumento")! == 3 &&
                (string)AnonymousObjectAssert.Prop(p, "Consecutivo")! == "00100001010000000001" &&
                (int)AnonymousObjectAssert.Prop(p, "EstadoEnvio")! == 1 &&
                (int)AnonymousObjectAssert.Prop(p, "EstadoHacienda")! == 2),
            "msj", It.IsAny<string>()), Times.Once);
    }
}
