namespace Bookify.Domain.Models
{
    public class RentalCopy
    {
        public int RentalId { get; set; }
        public Rental? Rental { get; set; }
        public int BookCopyId { get; set; }
        public BookCopy? BookCopy { get; set; }
        public DateTime RentalDate { get; set; } = DateTime.Today;
        public DateTime EndDate { get; set; } = DateTime.Today.AddDays((int)ApplicationEnums.RentalDuration);
        public DateTime? ReturnDate { get; set; }
        public DateTime? ExtendOn { get; set; }

        public int DelayInDays
        {
            get
            {
                return ReturnDate.HasValue && ReturnDate.Value > EndDate
                ? (int)(ReturnDate.Value - EndDate).TotalDays
                : !ReturnDate.HasValue && DateTime.Today > EndDate
                ? (int)(DateTime.Today - EndDate).TotalDays
                : 0;
            }
        }
    }
}
