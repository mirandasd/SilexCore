using SilexCore.Domain.Dtos;
using System.Data;

namespace SilexCore.Domain.Interfaces.Infraestructura.Persistence;

public interface IFacturaRepository
{
    Task<(int RowsAffected, string Mensaje)> AgregarDetalleFacturaAsync(AgregarDetalleFacturaRequest detalleFactura);
    Task<(int RowsAffected, string Mensaje)> AgregarExoneracionAsync(ExoneracionRequest exoneracion);
    Task<(int RowsAffected, string Mensaje)> AgregarFacturaAsync(AgregarFacturaRequest factura);
    Task<IEnumerable<ExoneracionResponse>> ObtenerExoneracionPorIdClienteAsync(int idCliente);
    Task<IEnumerable<FacturaPorConsecutivoResponse>> ObtenerFacturaPorConsecutivoYNegocioAsync(int idEntidad, string consecutivo);
    Task<FacturaResponse?> ObtenerFacturaPorIdAsync(int idFactura);
    Task<IEnumerable<DocumentosPorNegocioResponse>> ObtenerFacturasPorNegocioAsync(int idEntidad);
    Task<HistoricoFacturaDto?> ObtenerHistoricoFacturaPorIdAsync(int idFactura);




    Task<int> AgregarFacturaAsync(AgregarFacturaRequest factura, IDbTransaction transaction);
    Task<int> AgregarExoneracionAsync(ExoneracionRequest exoneracion, IDbTransaction transaction);
    Task AgregarDetalleFacturaAsync(AgregarDetalleFacturaRequest detalle, IDbTransaction transaction);
}
