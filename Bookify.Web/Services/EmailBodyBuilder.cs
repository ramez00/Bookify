namespace Bookify.Web.Services
{
    public class EmailBodyBuilder : IEmailBodyBuilder
    {
        private readonly IWebHostEnvironment _webHostEnvironment;

        public EmailBodyBuilder(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
        }

        public string getBuilder(string emailTemplate, Dictionary<string, string> placeholders)
        {
            var filePath = $"{_webHostEnvironment.WebRootPath}/templates/{emailTemplate}.html";

            StreamReader sr = new StreamReader(filePath);

            var template = sr.ReadToEnd();
            sr.Close();

            foreach (var item in placeholders)
                template = template.Replace($"[{item.Key}]", item.Value);

            return template;

        }
    }
}
