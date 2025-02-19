using Bookify.Domain.Common;

namespace Bookify.Domain.Models
{
    public class Rental : BaseEntity
    {
        public int Id { get; set; }
        public Subscriper? Subscriper { get; set; }
        public DateTime StartDate { get; set; } = DateTime.Now;
        public bool PentalyPaid { get; set; }
        public ICollection<RentalCopy> RentalCopies { get; set; } = new List<RentalCopy>();
    }
}
