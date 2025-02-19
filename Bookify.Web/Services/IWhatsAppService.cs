namespace Bookify.Web.Services
{
    public interface IWhatsAppService
    {
        Task<(bool isSent, string? ErrorMessage)> sendMessage(string mobileNumber, string msgType, string Name);
    }
}
