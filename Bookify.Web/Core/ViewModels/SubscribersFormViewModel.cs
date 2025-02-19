using UoN.ExpressiveAnnotations.NetCore.Attributes;

namespace Bookify.Web.Core.ViewModels
{
    public class SubscribersFormViewModel
    {
        public string? Key { get; set; }

        [Display(Name = "First Name"), MaxLength(100, ErrorMessage = Errors.MaxLength)]
        public string FirstName { get; set; } = null!;

        [Display(Name = "Last Name"), MaxLength(100, ErrorMessage = Errors.MaxLength)]
        public string LastName { get; set; } = null!;

        [Display(Name = "Date of Birth"), AssertThat("DateOfBirth <= Today()", ErrorMessage = Errors.InvalidDate)]
        public DateTime DateOfBirth { get; set; }

        [Display(Name = "National ID"), MaxLength(14, ErrorMessage = Errors.MaxLength),
            RegularExpression(RegexPattern.NationalID, ErrorMessage = Errors.InvalidNationalID),
            Remote("IsNationalIdExist", null, AdditionalFields = "Key", ErrorMessage = Errors.IsExist)]
        public string NationalId { get; set; } = null!;

        [Display(Name = "Mobile Number"), MaxLength(15, ErrorMessage = Errors.MaxLength),
            RegularExpression(RegexPattern.MobileNumber, ErrorMessage = Errors.InvalidMobileNumber),
            Remote("IsMobileExist", null, AdditionalFields = "Key", ErrorMessage = Errors.IsExist)]
        public string MobileNumber { get; set; } = null!;

        [Display(Name = "Has WhatsApp?")]
        public bool HasWhatsApp { get; set; }

        [EmailAddress, Display(Name = "Email"), MaxLength(150, ErrorMessage = Errors.MaxLength),
           Remote("IsEmailExist", null, AdditionalFields = "Key", ErrorMessage = Errors.IsExist)]
        public string Email { get; set; } = null!;

        [MaxLength(500)]
        public string? ImageUrl { get; set; }

        [MaxLength(500)]
        public string? ImageThumbnailUrl { get; set; }

        [RequiredIf("Key == null", ErrorMessage = Errors.EmptyImage)]
        public IFormFile? Image { get; set; }

        public int AreaId { get; set; }

        [Display(Name = "Area")]
        public IEnumerable<SelectListItem>? Areas { get; set; }

        public int GovernorateId { get; set; }

        [Display(Name = "Governorate")]
        public IEnumerable<SelectListItem>? Governorates { get; set; }

        [MaxLength(500)]
        public string Address { get; set; } = null!;
    }
}
