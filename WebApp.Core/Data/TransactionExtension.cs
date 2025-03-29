using Microsoft.EntityFrameworkCore;

namespace WebApp.Core.Data;

public static class TransactionExtension
{
    public static async Task ExecuteInTransaction(this AppDbContext dbContext, Func<Task> operations)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync();
        try
        {
            await operations();
            await transaction.CommitAsync();
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
    
    public static async Task SaveAndExecuteInTransaction(this AppDbContext dbContext, Func<Task> operations)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync();
        try
        {
            await operations();
            await dbContext.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}