namespace Bookify.Web.Core.ViewModels
{
    public class AuthorFormViewModel
    {
        public int Id { get; set; }

        [MaxLength(100, ErrorMessage = "The Maximum length 100 char."),
            Remote("IsExist", null, AdditionalFields = "Id", ErrorMessage = Errors.IsExist),
            RegularExpression(RegexPattern.EnglishCharaters, ErrorMessage = Errors.InvalidEngChar)]
        public string Name { get; set; } = null!;
    }
}
