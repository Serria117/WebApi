using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using WebApp.Core.DomainEntities;
using WebApp.Core.DomainEntities.Accounting;

namespace WebApp.Core.Data;

public class AppDbContext(DbContextOptions op) : DbContext(op)
{
    public DbSet<User> Users { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<Permission> Permissions { get; set; }
    public DbSet<Organization> Organizations { get; set; }
    public DbSet<OrganizationInfo> OrganizationInfos { get; set; }
    public DbSet<TaxOffice> TaxOffices { get; set; }
    public DbSet<District> Districts { get; set; }
    public DbSet<Province> Provinces { get; set; }
    public DbSet<Account> Accounts { get; set; }
    public DbSet<BalanceSheet> BalanceSheets { get; set; }
    public DbSet<BalanceSheetDetail> BalanceSheetDetails { get; set; }
    public DbSet<ImportedBalanceSheet> ImportedBalanceSheets { get; set; }
    public DbSet<ImportedBalanceSheetDetail> ImportedBalanceSheetDetails { get; set; }
    public DbSet<RiskCompany> RiskCompanies { get; set; }
    public DbSet<SyncInvoiceHistory> SyncInvoiceHistories { get; set; }
    public DbSet<OrgDocument> Documents { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasSequence<int>(name: "CommonSeq", schema: "dbo")
                    .StartsAt(1)
                    .IncrementsBy(1);
        
        modelBuilder.Entity<BalanceSheetDetail>()
                    .Property(b => b.Id)
                    .HasDefaultValueSql("NEXT VALUE FOR dbo.CommonSeq");

        modelBuilder.Entity<BalanceSheet>()
                    .HasMany<BalanceSheetDetail>(b => b.Details)
                    .WithOne(b => b.BalanceSheet)
                    .HasForeignKey("BlId").OnDelete(DeleteBehavior.Cascade);
        
        modelBuilder.Entity<ImportedBalanceSheet>()
                    .HasMany<ImportedBalanceSheetDetail>(b => b.Details)
                    .WithOne(b => b.ImportedBalanceSheet)
                    .HasForeignKey("BlId").OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<BalanceSheet>()
                    .HasOne<ImportedBalanceSheet>(b => b.ImportedBalanceSheet)
                    .WithOne(ib => ib.BalanceSheet)
                    .HasForeignKey<ImportedBalanceSheet>(i => i.BalanceSheetId)
                    .OnDelete(DeleteBehavior.Cascade);
        
        modelBuilder.Entity<Account>()
                    .Property(a => a.Id)
                    .ValueGeneratedNever();
        
        modelBuilder.Entity<District>()
                    .Navigation(d => d.Province)
                    .AutoInclude();
        
        base.OnModelCreating(modelBuilder);
        modelBuilder.FinalizeModel();
    }

    /*public override int SaveChanges()
    {
        AddAuditInfo();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = new())
    {
        AddAuditInfo();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void AddAuditInfo()
    {
        var entities = ChangeTracker.Entries()
                                    .Where(x => x is
                                    {
                                        Entity: IBaseEntityAuditable,
                                        State: EntityState.Added or EntityState.Modified
                                    });

        foreach (var entity in entities)
        {
            var now = DateTime.UtcNow.ToLocalTime(); // current datetime

            if (entity.State == EntityState.Added)
            {
                ((IBaseEntityAuditable)entity.Entity).CreateAt = now;
                ((IBaseEntityAuditable)entity.Entity).CreateBy = http.HttpContext?.User.Identity?.Name;
            }

            ((IBaseEntityAuditable)entity.Entity).LastUpdateAt = now;
        }
    }*/
}