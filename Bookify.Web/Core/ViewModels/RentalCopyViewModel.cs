namespace Bookify.Web.Core.ViewModels
{
    public class RentalCopyViewModel
    {
        public BookCopyViewModel? BookCopy { get; set; }
        public BookCopyViewModel? BookCopies { get; set; }
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

                //return (ReturnDate.HasValue && ReturnDate.Value > EndDate) 
                //? (int)(ReturnDate.Value - EndDate).TotalDays
                //: (!ReturnDate.HasValue && DateTime.Today > EndDate)
                //? (int)(DateTime.Today - EndDate).TotalDays
                //:0;
            }
        }
    }
}
