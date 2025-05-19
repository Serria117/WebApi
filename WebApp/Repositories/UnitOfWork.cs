using Microsoft.EntityFrameworkCore.Storage;
using WebApp.Core.Data;
using WebApp.Core.DomainEntities;

namespace WebApp.Repositories;

public interface IUnitOfWork
{
    /// <summary>
    /// Retrieves a repository instance for the specified entity type and key type.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity managed by the repository.</typeparam>
    /// <typeparam name="TKey">The type of the key for the entity.</typeparam>
    /// <returns>An instance of <see cref="IAppRepository{TEntity, TKey}"/> for the specified entity and key type.</returns>
    IAppRepository<TEntity, TKey> GetRepository<TEntity, TKey>() where TEntity : BaseEntity<TKey>;

    /// <summary>
    /// Begins a new database transaction asynchronously.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task BeginTransactionAsync();

    /// <summary>
    /// Asynchronously commits all changes made within the current unit of work to the database.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task CommitAsync();

    /// <summary>
    /// Asynchronously rolls back all changes made within the current transaction, reverting the database to its previous state.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task RollbackAsync();

    void Dispose();
    ValueTask DisposeAsync();
}

public class UnitOfWork(AppDbContext context, 
                        IServiceProvider serviceProvider) : IDisposable, IAsyncDisposable, IUnitOfWork
{
    private IDbContextTransaction? _transaction;
    private bool _disposed;
    public IAppRepository<TEntity, TKey> GetRepository<TEntity, TKey>() where TEntity : BaseEntity<TKey>
    {
        var repo = serviceProvider.GetService<IAppRepository<TEntity, TKey>>();
        if(repo == null) 
            throw new InvalidOperationException($"Repository {typeof(TEntity).Name} not found in DI container.");
        return repo;
    }

    public async Task BeginTransactionAsync()
    {
        if (_transaction != null) 
            throw new InvalidOperationException("Transaction already started.");
        _transaction = await context.Database.BeginTransactionAsync();
    }

    public async Task CommitAsync()
    {
        try
        {
            await context.SaveChangesAsync();
            if (_transaction != null)
            {
                await _transaction.CommitAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }
        catch
        {
            await RollbackAsync();
            throw;
        }
    }

    public async Task RollbackAsync()
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        context.Dispose();
        GC.SuppressFinalize(this);
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;
        if (_transaction != null)
        {
            await _transaction.DisposeAsync();
            _transaction = null;
        }
        await context.DisposeAsync();
        _disposed = true;
        GC.SuppressFinalize(this);
    }
}
