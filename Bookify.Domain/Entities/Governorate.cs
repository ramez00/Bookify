using Bookify.Domain.Common;

namespace Bookify.Domain.Models
{
    [Index(nameof(Name), IsUnique = true)]
    public class Governorate : BaseEntity
    {
        public int Id { get; set; }

        [MaxLength(100)]
        public string Name { get; set; } = null!;

        public ICollection<Area> Areas { get; set; } = new List<Area>();
    }
}
