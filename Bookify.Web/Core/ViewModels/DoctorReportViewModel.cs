using Bookify.Application.Common.Models;

namespace Bookify.Web.Core.ViewModels
{
    public class DoctorReportViewModel
    {
        public DateOnly From { get; set; } = new DateOnly();
        public DateOnly To { get; set; } = new DateOnly();
        public PaginatedList<RentalCopy> Rentals { get; set; }
        public int PageMode { get; set; }

    }
}
