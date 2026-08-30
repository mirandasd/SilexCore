using Karin.AuditService.Client.Dtos;
using Karin.AuditService.Client.Interface;
using Microsoft.Extensions.Configuration;
using SilexCore.Domain.Interfaces.Infraestructura.Services;

namespace SilexCore.Infrastructure.Services;

public class BitacoraService(IAuditServiceClient auditServiceClient, IConfiguration configuration) : IBitacoraService
{
    private readonly string servicio = configuration["Settings:Servicio"] ?? "SilexCore";

    public Task RegistrarNegocioBitacoraAsync(string mensaje, string payload)
        => auditServiceClient.Business(new AuditLogRequest(servicio, mensaje, payload));

    public Task RegistrarErrorBitacoraAsync(string mensaje, string payload)
        => auditServiceClient.Error(new AuditLogRequest(servicio, mensaje, payload));
}
