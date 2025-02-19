namespace Bookify.Web.Core.ViewModels
{
    public class RentalHistoryViewModel
    {
        public string? SubscriberFullName { get; set; }

        public string? SubscriberEmail { get; set; }

        public DateTime CreatedOn { get; set; }

        public DateTime RentalDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime? ReturnDate { get; set; }
        public DateTime? ExtendOn { get; set; }
        public int DelayInDays
        {
            get
            {
                var dely = 0;
                if (ReturnDate.HasValue && ReturnDate.Value > EndDate)
                    dely = (int)(ReturnDate.Value - EndDate).TotalDays;

                else if (!ReturnDate.HasValue && DateTime.Today > EndDate)
                    dely = (int)(DateTime.Today - EndDate).TotalDays;

                return dely;
            }
        }

    }
}
