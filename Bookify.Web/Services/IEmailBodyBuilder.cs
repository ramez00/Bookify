namespace Bookify.Web.Services
{
    public interface IEmailBodyBuilder
    {
        string getBuilder(string emailTemplate, Dictionary<string, string> placeholders);
    }
}
