using UoN.ExpressiveAnnotations.NetCore.Attributes;

namespace Bookify.Web.Core.ViewModels
{
    public class BookFormViewModel
    {
        public int Id { get; set; }

        [MaxLength(500, ErrorMessage = "The Maximum Length is 500 Chrs.")]
        [Remote("IsExist", null, AdditionalFields = "Id,AuthorId", ErrorMessage = "Book with Author is already Exist! ")]
        public string Title { get; set; } = null!;

        [Display(Name = "Author")]
        [Remote("IsExist", null, AdditionalFields = "Id,Title", ErrorMessage = "Author with defined Book is already Exist! ")]
        public int AuthorId { get; set; }

        public IEnumerable<SelectListItem>? Authors { get; set; }

        [MaxLength(200, ErrorMessage = "The Maximum Length is 200 Chrs.")]
        public string Publisher { get; set; } = null!;

        [Display(Name = "Publishing Date")]
        [AssertThat("PublishingDate <= Today()", ErrorMessage = "Date cannot be in the future!")]
        public DateTime PublishingDate { get; set; } = DateTime.Now;

        public string? ImageUrl { get; set; }
        public string? ImageThumbUrl { get; set; }
        public IFormFile? Image { get; set; }

        [MaxLength(50, ErrorMessage = "The Maximum Length is 50 Chrs.")]
        public string Hall { get; set; } = null!;

        [Display(Name = "Is available for rent?")]
        public bool IsAvailableForRent { get; set; }

        public string Description { get; set; } = null!;

        [Display(Name = "Categories")]
        public IList<int> SelectedCategories { get; set; } = new List<int>();

        public IEnumerable<SelectListItem>? Categories { get; set; }
    }
}
