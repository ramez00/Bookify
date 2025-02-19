
using Bookify.Infrastructure.Persistence.Repository;
using Bookify.Web.Data;

namespace Bookify.Infrastructure.Persistence
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
        }

        public IBaseRepository<Author> Authors => new BaseRepository<Author>(_context);

        public IBaseRepository<Category> Categories => new BaseRepository<Category>(_context);

        public int SaveChanges() => _context.SaveChanges();
    }
}
