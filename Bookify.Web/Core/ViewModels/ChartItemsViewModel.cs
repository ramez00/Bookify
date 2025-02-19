namespace Bookify.Domain.Models
{
    public class ChartItemsViewModel : RentalPerDayViewModel
    {
    }

    public class RentalPerDayViewModel
    {
        public string? Label { get; set; }
        public string? Value { get; set; } = null;
    }
}
