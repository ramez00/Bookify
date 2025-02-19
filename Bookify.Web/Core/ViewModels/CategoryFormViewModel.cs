namespace Bookify.Web.Core.ViewModels
{
    public class CategoryFormViewModel
    {
        public int Id { get; set; }

        [MaxLength(100, ErrorMessage = Errors.MaxLength),
            Remote("IsExist", null, AdditionalFields = "Id", ErrorMessage = Errors.IsExist)]
        public string Name { get; set; } = null!;
    }
}
