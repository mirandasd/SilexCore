using System.Data;
using Karin.Persistence.Dapper.Interfaces;
using Moq;
using SilexCore.Domain.Dtos;
using SilexCore.Domain.Interfaces.Infraestructura.Persistence;
using SilexCore.Infrastructure.Persistence;
using SilexCore.Tests.TestHelpers;

namespace SilexCore.Tests;

// RegistrarFacturaCompletaAsync es la transacción central de todo el flujo de
// facturación (cliente -> consecutivo -> factura -> detalle/exoneración ->
// actualizar consecutivo), probada en vivo varias veces esta sesión. Estas
// pruebas fijan sus reglas más frágiles con mocks: sobre todo el patrón
// "get-or-create no reporta filas afectadas" de AgregarExoneracionCompleta,
// que ya causó confusión real una vez (ver AgregarExoneracionCompleta en
// FacturaRepository.cs).
public class FacturaRepositoryTests
{
    private readonly Mock<ISqlExecutor> _sqlExecutor = new();
    private readonly IFacturaRepository _repo;

    public FacturaRepositoryTests()
    {
        _repo = new FacturaRepository(_sqlExecutor.Object);

        // ExecuteInTransactionAsync<T> normalmente abre una conexión/transacción
        // real; para la prueba simplemente se ejecuta el delegado recibido usando
        // el mismo mock como "tx", que es lo único que el código bajo prueba usa.
        _sqlExecutor
            .Setup(s => s.ExecuteInTransactionAsync(It.IsAny<Func<ISqlExecutor, Task<RegistrarFacturaResult>>>(), It.IsAny<string>(), It.IsAny<IsolationLevel>()))
            .Returns<Func<ISqlExecutor, Task<RegistrarFacturaResult>>, string, IsolationLevel>((operacion, _, _) => operacion(_sqlExecutor.Object));
    }

    private static RegistrarFacturaRequest RequestBase(int idClienteExistente = 10, params AgregarDetalleFacturaRequest[] lineas) => new()
    {
        IdEntidad = 1,
        IdTipoComprobante = 1,
        IdCondicionVenta = 1,
        IdMoneda = 1,
        IdMedioPago = 1,
        Cliente = new ClienteFacturaRequest { IdCliente = idClienteExistente },
        Lineas = lineas.Length > 0 ? [.. lineas] : [new AgregarDetalleFacturaRequest { Cantidad = 1, Precio = 100 }]
    };

    private void SetupConsecutivo(string consecutivo = "00100001010000000001") =>
        _sqlExecutor.Setup(s => s.QueryFirstOrDefaultAsync<string?>("sw_administracion.ObtenerConsecutivoPorNegocioYTipo", It.IsAny<object>(), It.IsAny<string>(), It.IsAny<CommandType>()))
            .ReturnsAsync(consecutivo);

    private void SetupAgregarFactura(int idFactura = 500, int rows = 1) =>
        _sqlExecutor.Setup(s => s.ExecuteSpWithOutputAsync("AgregarFactura", It.IsAny<object>(), "msj", It.IsAny<string>()))
            .ReturnsAsync((rows, idFactura.ToString()));

    private void SetupDetalle(int rows = 1) =>
        _sqlExecutor.Setup(s => s.ExecuteSpWithOutputAsync("AgregarDetalleFactura", It.IsAny<object>(), "msj", It.IsAny<string>()))
            .ReturnsAsync((rows, "ok"));

    private void SetupActualizarConsecutivo(int rows = 1) =>
        _sqlExecutor.Setup(s => s.ExecuteSpWithOutputAsync("sw_administracion.ActualizarConsecutivoPorRegistro", It.IsAny<object>(), "msj", It.IsAny<string>()))
            .ReturnsAsync((rows, "ok"));

    [Fact]
    public async Task RegistrarFacturaCompletaAsync_ClienteYaExistente_NoLlamaAgregarClienteMinimoExtendido()
    {
        SetupConsecutivo();
        SetupAgregarFactura();
        SetupDetalle();
        SetupActualizarConsecutivo();

        var resultado = await _repo.RegistrarFacturaCompletaAsync(RequestBase(idClienteExistente: 10));

        Assert.Equal(500, resultado.IdFactura);
        Assert.Equal(10, resultado.IdCliente);
        _sqlExecutor.Verify(s => s.ExecuteSpWithOutputAsync("sw_cliente.AgregarClienteMinimoExtendido", It.IsAny<object>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task RegistrarFacturaCompletaAsync_ClienteNuevo_LoCreaYUsaElIdDevuelto()
    {
        _sqlExecutor.Setup(s => s.ExecuteSpWithOutputAsync("sw_cliente.AgregarClienteMinimoExtendido", It.IsAny<object>(), "msj", It.IsAny<string>()))
            .ReturnsAsync((1, "77"));
        SetupConsecutivo();
        SetupAgregarFactura();
        SetupDetalle();
        SetupActualizarConsecutivo();

        var resultado = await _repo.RegistrarFacturaCompletaAsync(RequestBase(idClienteExistente: 0));

        Assert.Equal(77, resultado.IdCliente);
    }

    [Fact]
    public async Task RegistrarFacturaCompletaAsync_ClienteNuevoFallaAlCrearse_Lanza()
    {
        _sqlExecutor.Setup(s => s.ExecuteSpWithOutputAsync("sw_cliente.AgregarClienteMinimoExtendido", It.IsAny<object>(), "msj", It.IsAny<string>()))
            .ReturnsAsync((0, "Error: correo duplicado"));

        await Assert.ThrowsAsync<InvalidOperationException>(() => _repo.RegistrarFacturaCompletaAsync(RequestBase(idClienteExistente: 0)));
    }

    [Fact]
    public async Task RegistrarFacturaCompletaAsync_SinConsecutivoConfigurado_Lanza()
    {
        _sqlExecutor.Setup(s => s.QueryFirstOrDefaultAsync<string?>("sw_administracion.ObtenerConsecutivoPorNegocioYTipo", It.IsAny<object>(), It.IsAny<string>(), It.IsAny<CommandType>()))
            .ReturnsAsync((string?)null);

        await Assert.ThrowsAsync<InvalidOperationException>(() => _repo.RegistrarFacturaCompletaAsync(RequestBase()));
    }

    [Fact]
    public async Task RegistrarFacturaCompletaAsync_FallaAlRegistrarLaFactura_Lanza()
    {
        SetupConsecutivo();
        SetupAgregarFactura(rows: 0);

        await Assert.ThrowsAsync<InvalidOperationException>(() => _repo.RegistrarFacturaCompletaAsync(RequestBase()));
    }

    [Fact]
    public async Task RegistrarFacturaCompletaAsync_FallaElDetalle_Lanza()
    {
        SetupConsecutivo();
        SetupAgregarFactura();
        SetupDetalle(rows: 0);

        await Assert.ThrowsAsync<InvalidOperationException>(() => _repo.RegistrarFacturaCompletaAsync(RequestBase()));
    }

    [Fact]
    public async Task RegistrarFacturaCompletaAsync_FallaActualizarConsecutivoAlFinal_Lanza()
    {
        SetupConsecutivo();
        SetupAgregarFactura();
        SetupDetalle();
        SetupActualizarConsecutivo(rows: 0);

        await Assert.ThrowsAsync<InvalidOperationException>(() => _repo.RegistrarFacturaCompletaAsync(RequestBase()));
    }

    [Fact]
    public async Task RegistrarFacturaCompletaAsync_ExoneracionGetOrCreate_ConRowsAffectedEnCero_NoLanza()
    {
        // AgregarExoneracionCompleta es get-or-create: si la exoneración ya existía
        // no inserta nada y devuelve 0 filas afectadas, pero el id sigue viniendo
        // válido en el OUT -- el código debe confiar en el id parseado, no en rows.
        var linea = new AgregarDetalleFacturaRequest
        {
            Cantidad = 1,
            Precio = 100,
            Exoneracion = new ExoneracionRequest { Autorizacion = 123, PoseeCabys = false }
        };
        _sqlExecutor.Setup(s => s.ExecuteSpWithOutputAsync("AgregarExoneracionCompleta", It.IsAny<object>(), "msj", It.IsAny<string>()))
            .ReturnsAsync((0, "55")); // 0 filas, pero id real = 55 (ya existía)
        SetupConsecutivo();
        SetupAgregarFactura();
        SetupDetalle();
        SetupActualizarConsecutivo();

        var resultado = await _repo.RegistrarFacturaCompletaAsync(RequestBase(lineas: linea));

        Assert.Equal(500, resultado.IdFactura);
        _sqlExecutor.Verify(s => s.ExecuteSpWithOutputAsync("AgregarDetalleFactura",
            It.Is<object>(p => (int)AnonymousObjectAssert.Prop(p, "IdExoneracion")! == 55),
            "msj", It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task RegistrarFacturaCompletaAsync_ExoneracionConIdInvalido_Lanza()
    {
        var linea = new AgregarDetalleFacturaRequest
        {
            Cantidad = 1,
            Precio = 100,
            Exoneracion = new ExoneracionRequest { Autorizacion = 123 }
        };
        _sqlExecutor.Setup(s => s.ExecuteSpWithOutputAsync("AgregarExoneracionCompleta", It.IsAny<object>(), "msj", It.IsAny<string>()))
            .ReturnsAsync((0, "Error: no se pudo resolver el tipo de documento"));
        SetupConsecutivo();
        SetupAgregarFactura();

        await Assert.ThrowsAsync<InvalidOperationException>(() => _repo.RegistrarFacturaCompletaAsync(RequestBase(lineas: linea)));
    }

    [Fact]
    public async Task RegistrarFacturaCompletaAsync_ExoneracionConCabys_RegistraCadaCodigoCabys()
    {
        var linea = new AgregarDetalleFacturaRequest
        {
            Cantidad = 1,
            Precio = 100,
            Exoneracion = new ExoneracionRequest
            {
                Autorizacion = 123,
                PoseeCabys = true,
                Cabys =
                [
                    new ExoneracionCabysItem { CodigoCabys = "1111", DetalleCabys = "A" },
                    new ExoneracionCabysItem { CodigoCabys = "2222", DetalleCabys = "B" }
                ]
            }
        };
        _sqlExecutor.Setup(s => s.ExecuteSpWithOutputAsync("AgregarExoneracionCompleta", It.IsAny<object>(), "msj", It.IsAny<string>()))
            .ReturnsAsync((1, "9"));
        SetupConsecutivo();
        SetupAgregarFactura();
        SetupDetalle();
        SetupActualizarConsecutivo();

        await _repo.RegistrarFacturaCompletaAsync(RequestBase(lineas: linea));

        _sqlExecutor.Verify(s => s.ExecuteAsync("AgregarExoneracionCabys", It.IsAny<object>(), It.IsAny<string>(), It.IsAny<CommandType>()), Times.Exactly(2));
    }
}
