using System.Linq.Expressions;

namespace BisleriumBlog.Infrastructure.Interfaces.Repositories.Base;

public interface IGenericRepo
{
    #region Item Existence
    bool Exists<TEntity>(Expression<Func<TEntity, bool>> filter = null) where TEntity : class;
    #endregion

    #region Get Items Collection
    IEnumerable<TEntity> GetData<TEntity>(Expression<Func<TEntity, bool>> filter = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null, string includeProperties = "")
        where TEntity : class;

    IEnumerable<TEntity> GetPagedResult<TEntity>(int pageNumber, int pageSize, out int rowsCount,
        Expression<Func<TEntity, bool>> filter = null, string sortOn = "", bool isAscendingOrder = false,
        string includeProperties = "") where TEntity : class;

    IQueryable<TEntity> GetIQueryable<TEntity>(Expression<Func<TEntity, bool>> filter = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null, string includeProperties = "")
        where TEntity : class;
    #endregion

    #region Get Item
    TEntity GetById<TEntity>(object id) where TEntity : class;

    TEntity? GetFirstOrDefault<TEntity>(Expression<Func<TEntity, bool>> filter) where TEntity : class;
    #endregion

    #region Data Add
    Guid Add<TEntity>(TEntity entity) where TEntity : class;

    bool AddMultipleEntity<TEntity>(IEnumerable<TEntity> entityList) where TEntity : class;
    #endregion

    #region Data Edit
    void Edit<TEntity>(TEntity entityToUpdate) where TEntity : class;

    void EditMultipleEntity<TEntity>(IEnumerable<TEntity> entityList) where TEntity : class;
    #endregion

    #region Data Remove
    void Remove<TEntity>(object id) where TEntity : class;

    void RemoveMultipleEntity<TEntity>(Expression<Func<TEntity, bool>> filter) where TEntity : class;

    void Remove<TEntity>(TEntity entityToDelete) where TEntity : class;

    bool RemoveMultipleEntity<TEntity>(IEnumerable<TEntity> removeEntityList) where TEntity : class;
    #endregion
}
