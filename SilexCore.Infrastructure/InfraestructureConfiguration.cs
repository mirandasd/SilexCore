using Karin.AuditService.Client;
using Karin.AuthService.Client;
using Karin.HaciendaCatalogService.Client.Interface;
using Karin.InvoicingService.Client;
using Karin.MessagingService.Client;
using Karin.ReportingService.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Refit;
using SilexCore.Domain.Interfaces.Infraestructura.Persistence;
using SilexCore.Domain.Interfaces.Infraestructura.Services;
using SilexCore.Infrastructure.Persistence;
using SilexCore.Infrastructure.Services;

namespace SilexCore.Infrastructure;

public static class InfraestructureConfiguration
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        #region Persistence
        services.AddScoped<IClienteRepository, ClienteRepository>();
        services.AddScoped<IConsecutivoRepository, ConsecutivoRepository>();
        services.AddScoped<IFacturaRepository, FacturaRepository>();
        services.AddScoped<IRecepcionRepository, RecepcionRepository>();
        services.AddScoped<INotaRepository, NotaRepository>();
        services.AddScoped<IUsuarioRepository, UsuarioRepository>();
        services.AddScoped<IProduccionMaterialRepository, ProduccionMaterialRepository>();
        services.AddScoped<IOpcionVentaRepository, OpcionVentaRepository>();

        #endregion Persistence

        #region Cliente HTTP - Microservicio Hacienda (IIS)

        var haciendaServiceUrl = configuration["ServicesUrl:HaciendaCatalogService"]
            ?? throw new InvalidOperationException("La URL de HaciendaCatalogService no está configurada en appsettings.json");

        // Registrar el cliente Refit que viene en tu NuGet
        services.AddRefitClient<IHaciendaCatalogServiceClient>()
                .ConfigureHttpClient(c => c.BaseAddress = new Uri(haciendaServiceUrl));

        #endregion Cliente HTTP - Microservicio Hacienda (IIS)

        #region Clientes HTTP - Auth / Bitacora / Mensajeria

        var authServiceUrl = configuration["AuthServiceSettings:BaseUrl"]
            ?? throw new InvalidOperationException("La URL de AuthService no está configurada en appsettings.json");
        var auditServiceUrl = configuration["AuditServiceSettings:BaseUrl"]
            ?? throw new InvalidOperationException("La URL de AuditService no está configurada en appsettings.json");
        var messagingServiceUrl = configuration["MessagingServiceSettings:BaseUrl"]
            ?? throw new InvalidOperationException("La URL de MessagingService no está configurada en appsettings.json");
        var reportingServiceUrl = configuration["ReportingServiceSettings:BaseUrl"]
            ?? throw new InvalidOperationException("La URL de ReportingService no está configurada en appsettings.json");
        var invoicingServiceUrl = configuration["InvoicingServiceSettings:BaseUrl"]
            ?? throw new InvalidOperationException("La URL de InvoicingService no está configurada en appsettings.json");

        // Karin.AuthService.Client y Karin.InvoicingService.Client definen cada uno su
        // propio "AddAuthService" (el de InvoicingService quedó con ese nombre heredado
        // de su plantilla original, en realidad registra IInvoicingServiceClient) -- con
        // los dos "using" activos, el compilador no puede distinguirlos sin calificar la clase.
        Karin.AuthService.Client.ServiceCollectionExtensions.AddAuthService(services, authServiceUrl);
        services.AddAuditService(auditServiceUrl);
        services.AddMessagingService(messagingServiceUrl);
        services.AddReportingService(reportingServiceUrl);
        Karin.InvoicingService.Client.ServiceCollectionExtensions.AddAuthService(services, invoicingServiceUrl);

        #endregion Clientes HTTP - Auth / Bitacora / Mensajeria

        #region Servicios Internos
        services.AddScoped<IHaciendaCatalogServices, HaciendaCatalogServices>();
        services.AddScoped<IBitacoraService, BitacoraService>();
        services.AddScoped<IMensajeriaService, MensajeriaService>();
        services.AddScoped<IAutenticacionService, AutenticacionService>();
        services.AddScoped<IReporteService, ReporteService>();
        services.AddScoped<IImagenReporteService, ImagenReporteService>();
        services.AddScoped<IFacturaReporteService, FacturaReporteService>();
        services.AddScoped<IRecepcionReporteService, RecepcionReporteService>();
        services.AddScoped<INotaReporteService, NotaReporteService>();
        services.AddScoped<IFacturaEnvioService, FacturaEnvioService>();
        services.AddScoped<INotaEnvioService, NotaEnvioService>();
        services.AddScoped<IRecepcionEnvioService, RecepcionEnvioService>();
        services.AddScoped<ICertificadoConsultaService, CertificadoConsultaService>();

        #endregion Servicios Internos

        return services;
    }
}
