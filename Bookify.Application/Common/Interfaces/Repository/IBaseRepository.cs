
using Bookify.Domain.Constants;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;

namespace Bookify.Application.Common.Interfaces.Repository
{
    public interface IBaseRepository<T> where T : class
    {
      IEnumerable<T> GetAll(bool withNoTracking = true);
      T Add(T entity);
      T Update(T entity);
      T? GetById(int id);
      T? Find(Expression<Func<T,bool>> expression);
      T? Find(Expression<Func<T,bool>> expression, string[]? includes = null);
      T? Find(Expression<Func<T,bool>> expression,
          Func<IQueryable<T>,IIncludableQueryable<T,object>>? include = null);

      IEnumerable<T> FindAll(Expression<Func<T, bool>> expression,
            Expression<Func<T,object>>? orderBy = null, string OrderByDirection = OrderBy.Ascending);

      IEnumerable<T> FindAll(Expression<Func<T, bool>> expression,int? skip = null,int? take= null,
            Expression<Func<T, object>>? orderBy = null, string OrderByDirection = OrderBy.Ascending);

      IEnumerable<T> FindAll(Expression<Func<T, bool>> expression, int? skip = null, int? take = null,
         Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null,
            Expression<Func<T, object>>? orderBy = null, string OrderByDirection = OrderBy.Ascending);

        IEnumerable<T> AddRange(IEnumerable<T> entities);

        void Remove(T entity);
        void RemoveRange(IEnumerable<T> entities);

        bool IsExist(Expression<Func<T, bool>> expression);
        int Count();
        int Count(Expression<Func<T, bool>> expression);
        int Max(Expression<Func<T, bool>> expression, Expression<Func<T, int>> field);
    }
}
