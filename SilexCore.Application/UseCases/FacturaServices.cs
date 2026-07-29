using SilexCore.Domain.Dtos;
using SilexCore.Domain.Interfaces.Application;
using SilexCore.Domain.Interfaces.Infraestructura.Persistence;

namespace SilexCore.Application.UseCases;

public class FacturaServices(
    IUnitOfWorkFactory _unitOfWorkFactory,
    IFacturaRepository _facturaRepository,  
    IClienteRepository _clienteRepository,
    IConsecutivoRepository _consecutivoRepository
    )
{

    public async Task<RegistrarFacturaResult> EjecutarAsync(
     RegistrarFacturaRequest request, CancellationToken ct = default)
    {
        var uow = await _unitOfWorkFactory.CreateAsync();
        await using var _ = uow;

        try
        {
            var clienteId = await _clienteRepository.AgregarClienteAsync(
                request.Cliente, uow.Transaction);

            var facturaConCliente = request.Factura with { IdCliente = clienteId };
            var facturaId = await _facturaRepository.AgregarFacturaAsync(
                facturaConCliente, uow.Transaction);

            foreach (var linea in request.Detalles)
            {
                int? exoneracionId = null;

                if (linea.Exoneracion is not null)
                {
                    exoneracionId = await _facturaRepository.AgregarExoneracionAsync(
                        linea.Exoneracion, uow.Transaction);
                }

                var detalleConFactura = linea.Detalle with
                {
                    IdFactura = facturaId,
                    IdExoneracion = exoneracionId
                };

                await _facturaRepository.AgregarDetalleFacturaAsync(
                    detalleConFactura, uow.Transaction);
            }

            await _consecutivoRepository.ActualizarConsecutivoPorRegistroAsync(
                request.IdEntidad, request.TipoConsecutivo, uow.Transaction);

            await uow.CommitAsync();
            return new RegistrarFacturaResult(facturaId, "Factura registrada correctamente");
        }
        catch
        {
            await uow.RollbackAsync();
            throw;
        }
    }

}
