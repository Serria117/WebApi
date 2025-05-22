using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using WebApp.Core.DomainEntities;
using WebApp.Core.DomainEntities.Accounting;
using WebApp.Core.DomainEntities.Salary;

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

    public DbSet<Employee> Employees { get; set; } //
    public DbSet<PayrollPeriod> PayrollPeriods { get; set; }
    public DbSet<PayrollRecord> PayrollRecords { get; set; }
    public DbSet<PayrollComponentType> PayrollComponentTypes { get; set; } //
    public DbSet<PayrollComponentCategory> PayrollComponentCategories { get; set; } //
    public DbSet<PayrollItem> PayrollItems { get; set; }
    public DbSet<PayrollInput> PayrollInputs { get; set; } //
    public DbSet<PayrollInputType> PayrollInputTypes { get; set; }
    public DbSet<TimeSheet> TimeSheets { get; set; }
    public DbSet<BaseSalary> BaseSalaries { get; set; }
    public DbSet<Dependent> Dependents { get; set; }

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

        modelBuilder.Entity<PayrollRecord>()
                    .HasOne(pr => pr.PayrollPeriod)
                    .WithMany(pp => pp.PayrollRecords)
                    .HasForeignKey(pr => pr.PayrollPeriodId)
                    .IsRequired()
                    .OnDelete(DeleteBehavior.Restrict); // Không xóa PayrollPeriod khi xóa PayrollRecord

        // Composite unique index for PayrollRecord and Date
        modelBuilder.Entity<TimeSheet>()
                    .HasIndex(t => new { t.PayrollRecordId, t.Date }).IsUnique();
        // Relationships
        modelBuilder.Entity<TimeSheet>().HasOne(t => t.PayrollRecord)
                    .WithMany(pr => pr.TimeSheets)
                    .HasForeignKey(t => t.PayrollRecordId)
                    .OnDelete(DeleteBehavior.Cascade); // Nếu xóa PayrollRecord thì Timesheet liên quan cũng bị xóa

        modelBuilder.Entity<BaseSalary>(e =>
        {
            e.HasIndex(bs => new { bs.EmployeeId, bs.EffectiveDate }).IsUnique();
            e.HasOne(bs => bs.Employee)
             .WithMany(employee => employee.BaseSalaries)
             .HasForeignKey(bs => bs.EmployeeId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Dependent>(e =>
        {
            e.HasIndex(d => d.TaxId).IsUnique();
            
            e.HasOne(d => d.Employee)
             .WithMany(employee => employee.Dependents)
             .HasForeignKey(d => d.EmployeeId)
             .OnDelete(DeleteBehavior.Cascade);
        });
        
        base.OnModelCreating(modelBuilder);
        modelBuilder.FinalizeModel();
    }
}