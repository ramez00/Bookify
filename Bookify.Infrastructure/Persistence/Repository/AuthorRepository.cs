
using Bookify.Web.Data;
using Microsoft.EntityFrameworkCore;

namespace Bookify.Infrastructure.Persistence.Repository
{
    public class AuthorRepository : IAuthorRepository
    {
        private readonly IApplicationDbContext _context;

        public AuthorRepository(IApplicationDbContext context)
        {
            _context = context;
        }

        public Author Add(Author author)
        {
            _context.Authors.Add(author);
            _context.SaveChanges();

            return author;
        }

        public IEnumerable<Author> GetAll() => _context.Authors.AsNoTracking().ToList();

        public Author? GetById(int Id) => _context.Authors.Find(Id);

        public Author? GetByName(string name) => _context.Authors.SingleOrDefault(c => c.Name == name);

        public Author Update(Author author, string updatedUser)
        {
            author.LastUpdatedOn = DateTime.Now;
            author.LastUpdateById = updatedUser;

            _context.Authors.Update(author);
            _context.SaveChanges();
            return author;
        }

        public Author ToggleStatus(Author author,string updatedUser)
        {
            author.IsDeleted = !author.IsDeleted;
            author.LastUpdatedOn = DateTime.Now;
            author.LastUpdateById = updatedUser;

            _context.SaveChanges();

            return author;
        }
    }
}
