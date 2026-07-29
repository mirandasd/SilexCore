using Karin.Persistence.Dapper.Interfaces;
using SilexCore.Domain.Constants;
using SilexCore.Domain.Dtos;
using SilexCore.Domain.Interfaces.Infraestructura.Persistence;
using System.Data;

namespace SilexCore.Infrastructure.Persistence;

public class FacturaRepository(ISqlExecutor sqlExecutor): IFacturaRepository
{
    public Task<IEnumerable<DocumentosPorNegocioResponse>> ObtenerFacturasPorNegocioAsync(int idEntidad)
        => sqlExecutor.QueryAsync<DocumentosPorNegocioResponse>("ObtenerFacturasPorNegocio", new { IdEntidad = idEntidad }, connectionName: ConnectionNames.Default);

    public Task<IEnumerable<FacturaPorConsecutivoResponse>> ObtenerFacturaPorConsecutivoYNegocioAsync(int idEntidad, string consecutivo)
        => sqlExecutor.QueryAsync<FacturaPorConsecutivoResponse>("ObtenerFacturaPorConsecutivo", new { IdEntidad = idEntidad, Consecutivo = consecutivo }, connectionName: ConnectionNames.Default);

    public Task<FacturaResponse?> ObtenerFacturaPorIdAsync(int idFactura)
        => sqlExecutor.QueryFirstOrDefaultAsync<FacturaResponse?>("ObtenerFacturaPorId", new { IdFactura = idFactura }, connectionName: ConnectionNames.Default);

    public Task<HistoricoFacturaDto?> ObtenerHistoricoFacturaPorIdAsync(int idFactura)
        => sqlExecutor.QueryFirstOrDefaultAsync<HistoricoFacturaDto?>("ObtenerHistoricoFacturaPorId", new { IdFactura = idFactura }, connectionName: ConnectionNames.Default);

    public Task<(int RowsAffected, string Mensaje)> AgregarFacturaAsync(AgregarFacturaRequest factura)
        => sqlExecutor.ExecuteSpWithOutputAsync("AgregarFactura", factura, connectionName: ConnectionNames.Default);

    public Task<(int RowsAffected, string Mensaje)> AgregarDetalleFacturaAsync(AgregarDetalleFacturaRequest detalleFactura)
        => sqlExecutor.ExecuteSpWithOutputAsync("AgregarDetalleFactura", detalleFactura, connectionName: ConnectionNames.Default);

    public async Task<(int RowsAffected, string Mensaje)> AgregarExoneracionAsync(ExoneracionRequest exoneracion)
        => await sqlExecutor.ExecuteSpWithOutputAsync("AgregarExoneracion", exoneracion, connectionName: ConnectionNames.Default);

    public async Task<IEnumerable<ExoneracionResponse>> ObtenerExoneracionPorIdClienteAsync(int idCliente)
        => await sqlExecutor.QueryAsync<ExoneracionResponse>("ObtenerExoneracionPorIdCliente", new { IdCliente = idCliente }, connectionName: ConnectionNames.Default);

    public async Task<int> AgregarFacturaAsync(AgregarFacturaRequest factura, IDbTransaction transaction)
    {
        var (_, mensaje) = await transaction.ExecuteSpWithOutputAsync("AgregarFactura", factura);
        return int.Parse(mensaje);
    }

    public async Task<int> AgregarExoneracionAsync(ExoneracionRequest exoneracion, IDbTransaction transaction)
    {
        var (_, mensaje) = await transaction.ExecuteSpWithOutputAsync("AgregarExoneracion", exoneracion);
        return int.Parse(mensaje);
    }

    public Task AgregarDetalleFacturaAsync(AgregarDetalleFacturaRequest detalle, IDbTransaction transaction)
        => transaction.ExecuteSpAsync("AgregarDetalleFactura", detalle);
}
