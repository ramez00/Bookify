

namespace Bookify.Application.Common.Interfaces.Repository
{
    public interface IAuthorRepository
    {
        IEnumerable<Author> GetAll();
        Author Add(Author author);
        Author Update(Author author,string updatedUser);
        Author ToggleStatus(Author author,string updatedUser);
        Author? GetById(int Id);
        Author? GetByName(string name);
    }
}