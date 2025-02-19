namespace Bookify.Domain.Models
{
    public class Subscription
    {
        public int Id { get; set; }
        public Subscriper? subscripers { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public string? CreatedById { get; set; }

        public ApplicationUser? CreatedBy { get; set; }

        public DateTime CreatedOn { get; set; } = DateTime.Now;
    }
}
