using Bookify.Domain.Constants;
using Bookify.Web.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;

namespace Bookify.Infrastructure.Persistence.Repository
{
    public class BaseRepository<T> : IBaseRepository<T> where T : class
    {
        private readonly ApplicationDbContext _context;
        public BaseRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public IEnumerable<T> GetAll(bool withNoTracking = true)
        {
            IQueryable<T> query = _context.Set<T>();

            if (withNoTracking)
                query.AsNoTracking();

            return query.ToList();

        }
        public T Add(T entity)
        {
            _context.Set<T>().Add(entity);
            return entity;
        }
        public T Update(T entity)
        {
            _context.Set<T>().Update(entity);
            return entity;
        }
        public T? GetById(int id) => _context.Set<T>().Find(id);
        public T? Find(Expression<Func<T, bool>> expression) => _context.Set<T>().SingleOrDefault(expression);
        public T? Find(Expression<Func<T, bool>> expression, string[]? includes = null)
        {
            IQueryable<T> query = _context.Set<T>();

            if(includes is not null)
                foreach(var include in includes)
                    query = query.Include(include);

            return query.SingleOrDefault();
        }

        public T? Find(Expression<Func<T, bool>> expression,
            Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null)
        {
            IQueryable<T> query = _context.Set<T>().AsQueryable();

            if(include is not null)
                    query = include(query);

            return query.SingleOrDefault(expression);
        }

        public IEnumerable<T> FindAll(Expression<Func<T, bool>> expression,
            Expression<Func<T, object>>? orderBy = null,
            string OrderByDirection = "Ascending")
        {
            IQueryable<T> query = _context.Set<T>().Where(expression);

            if(orderBy is not null)
            {
                if(OrderByDirection == OrderBy.Ascending)
                    query = query.OrderBy(orderBy);
                else
                    query = query.OrderByDescending(orderBy);
            }

            return query.ToList();
        }

        public IEnumerable<T> FindAll(Expression<Func<T, bool>> expression,
            int? skip = null, int? take = null, Expression<Func<T, object>>? orderBy = null, 
            string OrderByDirection = OrderBy.Ascending)
        {
            IQueryable<T> query = _context.Set<T>().Where(expression);

            if(orderBy is not null)
            {
                if(OrderByDirection == OrderBy.Ascending)
                    query = query.OrderBy(orderBy);
                else
                    query = query.OrderByDescending(orderBy);
            }

            if (skip is not null)
                query = query.Skip(skip.Value);

            if(take is not null)
                query = query.Take(take.Value);

            return query.ToList();
        }

        public IEnumerable<T> FindAll(Expression<Func<T, bool>> expression, int? skip = null, int? take = null,
            Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null,
            Expression<Func<T, object>>? orderBy = null, string OrderByDirection = OrderBy.Ascending)
        {
            IQueryable<T> query = _context.Set<T>().AsQueryable();

            if (include is not null)
                query = include(query);

            query = query.Where(expression);

            if (orderBy is not null)
                if(OrderByDirection == OrderBy.Ascending)
                    query = query.OrderBy(orderBy);
                else
                    query = query.OrderByDescending(orderBy);

            if(skip is not null)
                query = query.Skip(skip.Value);

            if(take is not null)
                query = query.Take(take.Value);

            return query.ToList();
        }

        public IEnumerable<T> AddRange(IEnumerable<T> entities)
        {
            _context.AddRange(entities);
            return entities;
        }

        public void Remove(T entity) => _context.Remove(entity);

        public void RemoveRange(IEnumerable<T> entities) => _context.RemoveRange(entities);

        public bool IsExist(Expression<Func<T, bool>> expression) => _context.Set<T>().Any(expression);

        public int Count() => _context.Set<T>().Count();

        public int Count(Expression<Func<T, bool>> expression) => _context.Set<T>().Count(expression);

        public int Max(Expression<Func<T, bool>> expression, Expression<Func<T, int>> field) =>
            _context.Set<T>().Any(expression) ? _context.Set<T>().Where(expression).Max(field) : 0;
    }
}
