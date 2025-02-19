using UoN.ExpressiveAnnotations.NetCore.Attributes;

namespace Bookify.Web.Core.ViewModels
{
    public class UserFormViewModel
    {
        public string? Id { get; set; }

        [Display(Name = "Full Name"), MaxLength(100, ErrorMessage = Errors.MaxLength),
            RegularExpression(RegexPattern.EnglishCharaters, ErrorMessage = Errors.InvalidEngChar)]
        public string FullName { get; set; } = null!;

        [Display(Name = "User Name"), MaxLength(20, ErrorMessage = Errors.MaxLength),
            Remote("IsExist", null, AdditionalFields = "Id", ErrorMessage = Errors.IsExist),
            RegularExpression(RegexPattern.UserNamePattern, ErrorMessage = Errors.InvalidUserName)]
        public string UserName { get; set; } = null!;

        [EmailAddress, Display(Name = "Email"), MaxLength(200, ErrorMessage = Errors.MaxLength),
            Remote("IsExist", null, AdditionalFields = "Id", ErrorMessage = Errors.IsExist)]
        public string Email { get; set; } = null!;

        [Display(Name = "Password"), DataType(DataType.Password),
            StringLength(100, ErrorMessage = Errors.MaxMinLength, MinimumLength = 8),
            RegularExpression(RegexPattern.PasswordPattern, ErrorMessage = Errors.WeakPassword),
            RequiredIf("Id == null", ErrorMessage = Errors.RequiredField)]
        public string? Password { get; set; }

        [DataType(DataType.Password), Display(Name = "Confirm password"),
            Compare("Password", ErrorMessage = Errors.ConfirmPasswordNotMatch),
            RequiredIf("Id == null", ErrorMessage = Errors.RequiredField)]
        public string? ConfirmPassword { get; set; }

        [Display(Name = "Roles")]
        public IList<string> SelectedRoles { get; set; } = new List<string>();

        public IEnumerable<SelectListItem>? Roles { get; set; }
    }
}
