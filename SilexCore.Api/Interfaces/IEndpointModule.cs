namespace SilexCore.Api.Interfaces;

public interface IEndpointModule
{
    void RegistrarEndpoints(IEndpointRouteBuilder app);
}
