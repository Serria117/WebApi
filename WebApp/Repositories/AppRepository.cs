using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver.Linq;
using WebApp.Core.Data;
using WebApp.Core.DomainEntities;
using WebApp.Enums;
using Z.Expressions.Compiler;

namespace WebApp.Repositories;

public interface IAppRepository<T, in TK> where T : BaseEntity<TK>
{
    /// <summary>
    /// Creates a new entity in the database asynchronously.
    /// </summary>
    /// <param name="entity">The entity to be created.</param>
    /// <param name="inTransaction">Indicates whether the operation should be executed within a transaction.
    /// If the transaction is true, the saveChange() call will be handled by the transaction manager.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the created entity.</returns>
    Task<T> CreateAsync(T entity, bool inTransaction = false);

    /// <summary>
    /// Creates multiple entities in the database asynchronously.
    /// </summary>
    /// <param name="entities">A collection of entities to be created.</param>
    /// <param name="inTransaction">Indicates whether the creation should be executed within a transaction.
    /// If the transaction is true, the saveChange() call will be handled by the transaction manager.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task CreateManyAsync(IEnumerable<T> entities,
                         bool inTransaction = false,
                         CancellationToken cancellationToken = default);

    IQueryable<T> Find(Expression<Func<T, bool>> filter, params string[] include);

    IQueryable<T> Find(Expression<Func<T, bool>> filter, string? sortBy = "Id", string? order = SortOrder.ASC,
                       params string[] include);

    Task<T?> FindByIdAsync(TK id);
    Task<T> UpdateAsync(T entity, bool inTransaction = false);

    Task<ICollection<T>> UpdateManyAsync(ICollection<T> entities, bool inTransaction = false);
    Task<bool> ExistAsync(Expression<Func<T, bool>> predicate);
    Task<int> CountAsync(Expression<Func<T, bool>> predicate);
    Task<bool> SoftDeleteAsync(TK id);
    IQueryable<T> GetQueryable();
    Task<int> CountAsync();
    Task<bool> SoftDeleteManyAsync(params TK[] ids);
    IQueryable<T> FindAndSort(Expression<Func<T, bool>> filter, string[] include, string[] sortBy);
    T Attach(TK id);
    Task<bool> HardDeleteAsync(TK id, bool inTransaction = false);

    /// <summary>
    /// Permanently deletes multiple entities identified by their IDs from the database.
    /// </summary>
    /// <param name="ids">A collection of IDs representing the entities to delete.</param>
    /// <param name="inTransaction">Indicates whether the deletion should be executed within a transaction.
    /// If the transaction is true, the saveChange() call will be handled by the transaction manager.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task HardDeleteManyAsync(IEnumerable<TK> ids, bool inTransaction = false);
}

public class AppRepository<T, TK> : IAppRepository<T, TK> where T : BaseEntity<TK>, new()
{
    private readonly AppDbContext _db;
    private readonly DbSet<T> _dbSet;

    public AppRepository(AppDbContext dbContext)
    {
        _db = dbContext;
        _dbSet = _db.Set<T>();
    }

    public IQueryable<T> GetQueryable()
    {
        return _dbSet.AsQueryable();
    }

    public T Attach(TK id)
    {
        return _dbSet.Attach(new T { Id = id }).Entity;
    }

    public async Task<T> CreateAsync(T entity, bool inTransaction = false)
    {
        var saved = (await _dbSet.AddAsync(entity)).Entity;
        if (!inTransaction) await _db.SaveChangesAsync();

        return saved;
    }

    public IQueryable<T> Find(Expression<Func<T, bool>> filter, params string[] include)
    {
        var query = _dbSet.Where(filter);
        if (!include.IsNullOrEmpty())
        {
            query = include.Aggregate(query, (current, prop) => current.Include(prop))
                           .AsSplitQuery();
        }

        return query.OrderBy("Id DESC");
    }

    public IQueryable<T> Find(Expression<Func<T, bool>> filter, string? sortBy = "Id", string? order = "DESC",
                              params string[] include)
    {
        var query = _dbSet.Where(filter);
        if (!include.IsNullOrEmpty())
        {
            query = include.Aggregate(query, (current, prop) => current.Include(prop));
        }

        return query.OrderBy($"{sortBy} {order}");
    }

    public IQueryable<T> FindAndSort(Expression<Func<T, bool>> filter, string[] include, string[] sortBy)
    {
        var query = _dbSet.Where(filter);
        if (!include.IsNullOrEmpty())
        {
            query = include.Aggregate(query, (current, prop) => current.Include(prop));
        }

        if (!sortBy.IsNullOrEmpty())
        {
            query = sortBy.Aggregate(query, (current, sort) => current.OrderBy(sort));
        }

        return query;
    }

    public async Task CreateManyAsync(IEnumerable<T> entities,
                                      bool inTransaction = false,
                                      CancellationToken cancellationToken = default)
    {
        await _dbSet.AddRangeAsync(entities, cancellationToken);
        if (!inTransaction)
            await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task<T?> FindByIdAsync(TK id)
    {
        return await _dbSet.FindAsync(id);
    }

    public async Task<T> UpdateAsync(T entity, bool inTransaction = false)
    {
        var res = _dbSet.Update(entity);
        if (!inTransaction) await _db.SaveChangesAsync();
        return res.Entity;
    }

    public async Task<ICollection<T>> UpdateManyAsync(ICollection<T> entities, bool inTransaction = false)
    {
        _dbSet.UpdateRange(entities);
        if (!inTransaction) await _db.SaveChangesAsync();
        return entities;
    }
    

    public async Task<bool> ExistAsync(Expression<Func<T, bool>> predicate)
    {
        return await _dbSet.Where(predicate).AsNoTracking().AnyAsync();
    }

    public async Task<int> CountAsync(Expression<Func<T, bool>> predicate)
    {
        return await _dbSet.Where(predicate).AsNoTracking().CountAsync();
    }

    public async Task<int> CountAsync()
    {
        return await _dbSet.AsNoTracking().CountAsync();
    }

    public async Task<bool> SoftDeleteAsync(TK id)
    {
        var result = await Find(x => x.Id != null && x.Id.Equals(id) && !x.Deleted)
            .ExecuteUpdateAsync(x => x.SetProperty(p => p.Deleted, true));
        return result > 0;
    }

    public async Task<bool> SoftDeleteManyAsync(params TK[] ids)
    {
        var result = await _dbSet.Where(x => ids.Contains(x.Id) && !x.Deleted)
                                 .ExecuteUpdateAsync(x => x.SetProperty(p => p.Deleted, true));
        return result > 0;
    }

    public async Task<bool> HardDeleteAsync(TK id, bool inTransaction = false)
    {
        var entity = await _dbSet.FindAsync(id);
        if (entity == null) return false;
        _dbSet.Remove(entity);
        if (!inTransaction) await _db.SaveChangesAsync();
        return true;
    }

    public async Task HardDeleteManyAsync(IEnumerable<TK> ids, bool inTransaction = false)
    {
        var entities = await _dbSet.Where(x => ids.Contains(x.Id)).ToListAsync();
        _dbSet.RemoveRange(entities);
        if (!inTransaction) await _db.SaveChangesAsync();
    }
}