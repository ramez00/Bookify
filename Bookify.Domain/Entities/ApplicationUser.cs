namespace Bookify.Domain.Models
{
    [Index(nameof(Email), IsUnique = true)]
    [Index(nameof(UserName), IsUnique = true)]
    public class ApplicationUser : IdentityUser
    {
        [MaxLength(100)]
        public string FullName { get; set; } = null!;

        public bool IsDeleted { get; set; }

        public string? CreatedById { get; set; }

        public DateTime CreatedOn { get; set; } = DateTime.Now;

        public string? LastUpdateById { get; set; }

        public DateTime? LastUpdatedOn { get; set; }
    }
}
