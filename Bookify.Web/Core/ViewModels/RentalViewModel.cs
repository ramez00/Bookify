namespace Bookify.Web.Core.ViewModels
{
    public class RentalViewModel
    {
        public int Id { get; set; }
        public SubscriberViewMOdel? Subscriber { get; set; }
        public DateTime StartDate { get; set; } = DateTime.Now;
        public bool PenatlyPaid { get; set; }
        public IEnumerable<RentalCopyViewModel> RentalCopies { get; set; } = new List<RentalCopyViewModel>();
        public DateTime CreatedOn { get; set; }
        public int TotalDelayInDays
        {
            get
            {
                return RentalCopies.Sum(x => x.DelayInDays);
            }
        }
        public int TotalNumberCopies
        {
            get
            {
                return RentalCopies.Count();
            }
        }
    }
}
