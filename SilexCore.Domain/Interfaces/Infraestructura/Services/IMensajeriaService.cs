namespace SilexCore.Domain.Interfaces.Infraestructura.Services;

public interface IMensajeriaService
{
    Task EnviarNotificacionAsync(string destinatario, string asunto, string cuerpo);
}
