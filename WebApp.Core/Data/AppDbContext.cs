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
    public DbSet<OrganizationLoginInfo> OrganizationLoginInfos { get; set; }

    public DbSet<UserLog> UserLogs { get; set; }

    public DbSet<MenuItem> MenuItems { get; set; }
    public DbSet<MenuPermission> MenuPermissions { get; set; }

    public DbSet<EmailConfig> EmailConfigs { get; set; }

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

        modelBuilder.Entity<Permission>()
                    .HasIndex(p => p.PermissionName)
                    .IsUnique();

        modelBuilder.Entity<Organization>()
                    .HasMany<OrganizationLoginInfo>(o => o.OrganizationLoginInfos)
                    .WithOne(i => i.Organization)
                    .HasForeignKey(i => i.OrganizationId)
                    .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<MenuItem>()
                    .HasOne(m => m.Parent)
                    .WithMany(p => p.Items)
                    .HasForeignKey(m => m.ParentId)
                    .IsRequired(false);

        modelBuilder.Entity<MenuPermission>()
                    .HasKey(mp => new
                    {
                        mp.MenuId, mp.PermissionId
                    }); // Khóa chính của bảng MenuPermission là MenuId và PermissionId

        modelBuilder.Entity<MenuPermission>()
                    .HasOne(mp => mp.MenuItem) // Một MenuPermission thuộc về một Menu
                    .WithMany(m => m.MenuPermissions) // Một Menu có nhiều MenuPermission
                    .HasForeignKey(mp => mp.MenuId); // Khóa ngoại là MenuId

        modelBuilder.Entity<MenuPermission>()
                    .HasOne(mp => mp.Permission) // Một MenuPermission thuộc về một Permission
                    .WithMany(p => p.MenuPermissions) // Một Permission có nhiều MenuPermission
                    .HasForeignKey(mp => mp.PermissionId);
        
        base.OnModelCreating(modelBuilder);
        modelBuilder.FinalizeModel();
    }
}