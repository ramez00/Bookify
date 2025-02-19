using Bookify.Application.Common.Interfaces.Repository;

namespace Bookify.Application.Common.Interfaces
{
    public interface IUnitOfWork
    {
        IBaseRepository<Author> Authors { get; }
        IBaseRepository<Category> Categories { get; }
        int SaveChanges();
    }
}
