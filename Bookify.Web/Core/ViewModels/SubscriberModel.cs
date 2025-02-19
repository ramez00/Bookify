namespace Bookify.Web.Core.ViewModels
{
    public class SubscriberModel
    {
        public string? Key { get; set; }
        public int Id { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? ImageThumbnailUrl { get; set; }
        public string? DateofBirth { get; set; }
        public string? NationalId { get; set; }
        public string? MobileNumber { get; set; }
        public string? HasWhatsApp { get; set; }
        public string? ImageUrl { get; set; }
        public string? Area { get; set; }
        public string? Governrate { get; set; }
        public string? Address { get; set; }
        public DateTime CreatedOn { get; set; }

        public bool IsBlackListed { get; set; }

        public IEnumerable<SubscribtionViewModel> Subscriptions { get; set; } = new List<SubscribtionViewModel>();
        public IEnumerable<RentalViewModel> Rentals { get; set; } = new List<RentalViewModel>();
    }
}
