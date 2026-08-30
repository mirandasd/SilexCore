using Karin.MessagingService.Client.Dtos;
using Karin.MessagingService.Client.Interface;
using SilexCore.Domain.Interfaces.Infraestructura.Services;

namespace SilexCore.Infrastructure.Services;

public class MensajeriaService(IMessagingServiceClient messagingServiceClient) : IMensajeriaService
{
    public Task EnviarNotificacionAsync(string destinatario, string asunto, string cuerpo)
        => messagingServiceClient.SendMessageAsync(new CreateMessageRequest("SilexCore", destinatario, asunto, cuerpo));
}
