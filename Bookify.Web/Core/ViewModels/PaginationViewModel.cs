namespace Bookify.Web.Core.ViewModels
{
    public class PaginationViewModel
    {
        public int PageNumber { get; set; }
        public int TotalPages { get; set; }

        public int NextPage => PageNumber + 1;
        public int PreviousPage => PageNumber - 1;

        public bool HasPreviousPage => PageNumber > 1;

        public bool HasNextPage => PageNumber < TotalPages;
    }
}
