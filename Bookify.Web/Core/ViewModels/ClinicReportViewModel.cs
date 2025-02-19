using Bookify.Application.Common.Models;

namespace Bookify.Web.Core.ViewModels
{
    public class ClinicReportViewModel
    {
        public List<int>? SelectedAuthor { get; set; } = new List<int>();
        public List<int>? SelectedCategories { get; set; } = new List<int>();

        [Display(Name = "Author")]
        public IEnumerable<SelectListItem> Authors { get; set; } = new List<SelectListItem>();

        [Display(Name = "Categories")]
        public IEnumerable<SelectListItem> Categories { get; set; } = new List<SelectListItem>();
        public PaginatedList<Book> Books { get; set; }
    }
}
