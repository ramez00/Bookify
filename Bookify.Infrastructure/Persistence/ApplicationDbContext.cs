
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Bookify.Web.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>, IApplicationDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<Category> Categories { get; set; }

        public DbSet<Author> Authors { get; set; }

        public DbSet<Book> Books { get; set; }

        public DbSet<BookCategory> BookCategories { get; set; }

        public DbSet<BookCopy> BookCopies { get; set; }

        public DbSet<Area> Areas { get; set; }

        public DbSet<Governorate> Governorates { get; set; }

        public DbSet<Subscriper> Subscripers { get; set; }

        public DbSet<Subscription> Subscriptions { get; set; }
        public DbSet<Rental> Rentals { get; set; }
        public DbSet<RentalCopy> RentalCopies { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            // to Rename Table in dBase *******

            //builder.Entity<IdentityUser>().ToTable("Users");  
            //builder.Entity<IdentityRole>().ToTable("Rokes");  

            // to remove column from  entity ****

            //builder.Entity<IdentityUser>().Ignore(e => e.PhoneNumber).Ignore(e => e.PhoneNumberConfirmed);

            builder.HasSequence<int>(nameof(BookCopy.SerialNumber))
                .StartsAt(1000001);

            builder.Entity<BookCopy>()
                .Property(e => e.SerialNumber)
                .HasDefaultValueSql("NEXT VALUE FOR SerialNumber");

            // Define Composite keys

            builder.Entity<BookCategory>().HasKey(b => new { b.BookId, b.CategoryId });
            builder.Entity<RentalCopy>().HasKey(r => new { r.RentalId, r.BookCopyId });

            // Global Query Filter Need filter applied when I select from table

            builder.Entity<Rental>().HasQueryFilter(r => !r.IsDeleted);
            builder.Entity<RentalCopy>().HasQueryFilter(r => !r.Rental!.IsDeleted);

            // when Delete From Table Make Restriction For all Sub entities Related to Deleted Master Table 

            var cascadedFk = builder.Model.GetEntityTypes()
                .SelectMany(t => t.GetForeignKeys())
                .Where(fk => fk.DeleteBehavior == DeleteBehavior.Cascade && !fk.IsOwnership);

            foreach (var FK in cascadedFk)
                FK.DeleteBehavior = DeleteBehavior.Restrict;

            base.OnModelCreating(builder);

        }

    }
}
