namespace Bookify.Domain.Models
{
    [Index(nameof(Name), nameof(GovernorateId), IsUnique = true)]
    public class Area
    {
        public int Id { get; set; }

        [MaxLength(100)]
        public string Name { get; set; } = null!;

        public int GovernorateId { get; set; }

        public Governorate? Governorate { get; set; }

    }
}
