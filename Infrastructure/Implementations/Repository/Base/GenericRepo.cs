using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Linq;
using BisleriumBlog.Infrastructure.Persistence;
using BisleriumBlog.Infrastructure.Interfaces.Repositories.Base;

namespace BisleriumBlog.Infrastructure.Implementations.Repository.Base;

public class GenericRepo(ApplicationDbContext dbContext) : IGenericRepo
{
    public IEnumerable<TEntity> GetData<TEntity>(Expression<Func<TEntity, bool>>? filter = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null, string includeProperties = "")
        where TEntity : class
    {
        try
        {
            IQueryable<TEntity> query = dbContext.Set<TEntity>();

            if (filter != null) query = query.Where(filter);

            query = includeProperties.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Aggregate(query, (current, includeProperty) => current.Include(includeProperty));

            return orderBy != null ? orderBy(query).ToList() : query.ToList();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public IEnumerable<TEntity> GetPagedResult<TEntity>(int pageNum, int pageSize, out int rowsCount,
        Expression<Func<TEntity, bool>> filter = null, string sortOn = "", bool isAscendingOrder = false,
        string includeProperties = "") where TEntity : class
    {
        throw new NotImplementedException();
    }

    public IQueryable<TEntity> GetIQueryable<TEntity>(Expression<Func<TEntity, bool>>? filter = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null, string includeProperties = "")
        where TEntity : class
    {
        try
        {
            var query = dbContext.Set<TEntity>().AsNoTracking();

            if (filter != null) query = query.Where(filter);

            query = includeProperties.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Aggregate(query, (current, includeProperty) => current.Include(includeProperty));

            return orderBy != null ? orderBy(query) : query;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public TEntity? GetById<TEntity>(object? id) where TEntity : class
    {
        try
        {
            return dbContext.Set<TEntity>().Find(id);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public TEntity? GetFirstOrDefault<TEntity>(Expression<Func<TEntity, bool>> filter) where TEntity : class
    {
        try
        {
            return dbContext.Set<TEntity>().FirstOrDefault(filter);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public bool Exists<TEntity>(Expression<Func<TEntity, bool>>? filter = null) where TEntity : class
    {
        try
        {
            return filter != null && dbContext.Set<TEntity>().Any(filter);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public Guid Add<TEntity>(TEntity entity) where TEntity : class
    {
        try
        {
            ArgumentNullException.ThrowIfNull(entity);

            dbContext.Set<TEntity>().Add(entity);
            dbContext.SaveChanges();

            var result = Guid.Empty;

            var key = typeof(TEntity).GetProperties().FirstOrDefault(p =>
                p.CustomAttributes.Any(attr => attr.AttributeType == typeof(KeyAttribute)));

            if (key != null && key.PropertyType == typeof(Guid))
            {
                result = (Guid)key.GetValue(entity, null);
            }

            return result;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public bool AddMultipleEntity<TEntity>(IEnumerable<TEntity> entityList) where TEntity : class
    {
        try
        {
            ArgumentNullException.ThrowIfNull(entityList);

            dbContext.Set<TEntity>().AddRange(entityList);
            dbContext.SaveChanges();

            const bool flag = true;
            return flag;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public void Edit<TEntity>(TEntity entityToUpdate) where TEntity : class
    {
        try
        {
            ArgumentNullException.ThrowIfNull(entityToUpdate);

            dbContext.Entry(entityToUpdate).State = EntityState.Modified;
            dbContext.SaveChanges();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public void EditMultipleEntity<TEntity>(IEnumerable<TEntity> entityList) where TEntity : class
    {
        try
        {
            ArgumentNullException.ThrowIfNull(entityList);

            dbContext.Entry(entityList).State = EntityState.Modified;
            dbContext.SaveChanges();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public void Remove<TEntity>(object id) where TEntity : class
    {
        try
        {
            var entityToDelete = dbContext.Set<TEntity>().Find(id);

            if (entityToDelete == null) throw new ArgumentNullException("Entity");

            dbContext.Set<TEntity>().Remove(entityToDelete);
            dbContext.SaveChanges();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public void RemoveMultipleEntity<TEntity>(Expression<Func<TEntity, bool>> filter) where TEntity : class
    {
        try
        {
            var query = dbContext.Set<TEntity>().Where(filter);

            dbContext.Set<TEntity>().RemoveRange(query);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public void Remove<TEntity>(TEntity entityToDelete) where TEntity : class
    {
        try
        {
            ArgumentNullException.ThrowIfNull(entityToDelete);

            dbContext.Set<TEntity>().Remove(entityToDelete);
            dbContext.SaveChanges();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public bool RemoveMultipleEntity<TEntity>(IEnumerable<TEntity> removeEntityList) where TEntity : class
    {
        try
        {
            ArgumentNullException.ThrowIfNull(removeEntityList);

            dbContext.Set<TEntity>().RemoveRange(removeEntityList);
            dbContext.SaveChanges();

            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}
