using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using WebApp.Core.DomainEntities;

namespace WebApp.Core.Data;

public sealed class AuditableEntityInterceptor(IHttpContextAccessor http) : SaveChangesInterceptor
{
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is null) return base.SavingChangesAsync(eventData, result, cancellationToken);
        var entries = eventData.Context.ChangeTracker.Entries<IBaseEntityAuditable>();
        var now = DateTime.UtcNow;
        foreach (var entry in entries)
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    Console.WriteLine("Writing audited information for added entity.");
                    entry.Entity.CreateAt = now.ToLocalTime();
                    entry.Entity.LastUpdateAt = now.ToLocalTime();
                    entry.Entity.CreateBy = http.HttpContext?.User.Identity?.Name;
                    break;
                case EntityState.Modified:
                    Console.WriteLine("Writing audited information for modified entity.");
                    entry.Entity.LastUpdateAt = now.ToLocalTime();
                    break;
            }
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }
}