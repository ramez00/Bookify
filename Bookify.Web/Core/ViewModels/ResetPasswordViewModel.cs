namespace Bookify.Web.Core.ViewModels
{
    public class ResetPasswordViewModel
    {
        public string Id { get; set; } = null!;

        [Display(Name = "Password"), DataType(DataType.Password),
           StringLength(100, ErrorMessage = Errors.MaxMinLength, MinimumLength = 8),
           RegularExpression(RegexPattern.PasswordPattern, ErrorMessage = Errors.WeakPassword)]
        public string Password { get; set; } = null!;

        [DataType(DataType.Password), Display(Name = "Confirm password"),
            Compare("Password", ErrorMessage = Errors.ConfirmPasswordNotMatch)]
        public string ConfirmPassword { get; set; } = null!;

    }
}
