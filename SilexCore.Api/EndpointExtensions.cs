using SilexCore.Api.Interfaces;

namespace SilexCore.Api;

public static class EndpointExtensions
{
    public static IEndpointRouteBuilder MapEndpointModules(this IEndpointRouteBuilder app)
    {
        var moduleTypes = typeof(Program).Assembly.GetTypes()
            .Where(t => typeof(IEndpointModule).IsAssignableFrom(t)
                     && !t.IsInterface
                     && !t.IsAbstract);

        foreach (var type in moduleTypes)
        {
            if (Activator.CreateInstance(type) is IEndpointModule module)
            {
                module.RegistrarEndpoints(app);
            }
        }

        return app;
    }
}
