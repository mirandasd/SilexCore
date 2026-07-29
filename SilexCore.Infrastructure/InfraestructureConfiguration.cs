using Karin.HaciendaCatalogService.Client.Interface;
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

        #endregion Persistence

        #region Cliente HTTP - Microservicio Hacienda (IIS)

        var haciendaServiceUrl = configuration["ServicesUrl:HaciendaCatalogService"]
            ?? throw new InvalidOperationException("La URL de HaciendaCatalogService no está configurada en appsettings.json");

        // Registrar el cliente Refit que viene en tu NuGet
        services.AddRefitClient<IHaciendaCatalogServiceClient>()
                .ConfigureHttpClient(c => c.BaseAddress = new Uri(haciendaServiceUrl));

        #endregion Cliente HTTP - Microservicio Hacienda (IIS)

        #region Servicios Internos
        services.AddScoped<IHaciendaCatalogServices, HaciendaCatalogServices>();

        #endregion Servicios Internos

        return services;
    }
}
