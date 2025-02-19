using WhatsAppCloudApi;
using WhatsAppCloudApi.Services;

namespace Bookify.Web.Services
{
    public class WhatsAppService : IWhatsAppService
    {
        private readonly IWhatsAppClient _whatsAppClient;

        public WhatsAppService(IWhatsAppClient whatsAppClient)
        {
            _whatsAppClient = whatsAppClient;
        }

        public async Task<(bool isSent, string? ErrorMessage)> sendMessage(string mobileNumber, string msgType, string Name)
        {
            var comp = new List<WhatsAppComponent>()
            {
                new WhatsAppComponent
                {
                    Type = "body",
                    Parameters = new List<object>()
                    {
                        new WhatsAppTextParameter {Text = Name}
                    }
                }
            };

            var mobNumber = $"2{mobileNumber}";

            var s = await _whatsAppClient
                 .SendMessage(mobNumber, WhatsAppLanguageCode.English_US, WhatsAppTemplates.WelcomeMsg, comp);

            if (s!.Error is not null)
                return (isSent: false, ErrorMessage: s.Error.ToString());

            return (isSent: true, ErrorMessage: null);
        }
    }
}
