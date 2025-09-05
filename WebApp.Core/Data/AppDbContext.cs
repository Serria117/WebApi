using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using WebApp.Core.DomainEntities;
using WebApp.Core.DomainEntities.Accounting;
using WebApp.Core.DomainEntities.Payroll;

namespace WebApp.Core.Data;

public class AppDbContext(DbContextOptions op) : DbContext(op)
{
    public DbSet<User> Users { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<Permission> Permissions { get; set; }

    public DbSet<TaxOffice> TaxOffices { get; set; }
    public DbSet<TaxOffice2> TaxOffices2 { get; set; }
    public DbSet<District> Districts { get; set; }
    public DbSet<Province> Provinces { get; set; }

    //public DbSet<Account> Accounts { get; set; }
    //public DbSet<BalanceSheet> BalanceSheets { get; set; }
    //public DbSet<BalanceSheetDetail> BalanceSheetDetails { get; set; }
    //public DbSet<ImportedBalanceSheet> ImportedBalanceSheets { get; set; }
    //public DbSet<ImportedBalanceSheetDetail> ImportedBalanceSheetDetails { get; set; }

    public DbSet<RiskCompany> RiskCompanies { get; set; }
    public DbSet<InvoiceHistory> SyncInvoiceHistories { get; set; }

    public DbSet<Template> Templates { get; set; }
    public DbSet<TemplateFile> TemplateFiles { get; set; }
    public DbSet<OrgDocument> Documents { get; set; }

    public DbSet<Organization> Organizations { get; set; }
    public DbSet<OrganizationLoginInfo> OrganizationLoginInfos { get; set; }
    public DbSet<OrganizationInfo> OrganizationInfos { get; set; }

    public DbSet<TaxProcedure> TaxProcedures { get; set; }

    public DbSet<JobSetting> JobSettings { get; set; }

    public DbSet<UserLog> UserLogs { get; set; }

    public DbSet<MenuItem> MenuItems { get; set; }
    public DbSet<MenuPermission> MenuPermissions { get; set; }

    public DbSet<EmailConfig> EmailConfigs { get; set; }
    public DbSet<EmailAttachment> EmailAttachments { get; set; }
    public DbSet<EmailSenderAddress> EmailSenderAddresses { get; set; }

    public DbSet<InvoiceServiceToken> InvoiceServiceTokens { get; set; }

    // Payroll related entities
    public DbSet<Employee> Employees { get; set; }
    public DbSet<Dependents> Dependents { get; set; }
    public DbSet<Salary> Salaries { get; set; }
    public DbSet<Allowance> Allowances { get; set; }
    public DbSet<AllowanceType> AllowanceTypes { get; set; }
    public DbSet<Bonus> Bonus { get; set; }
    public DbSet<BonusType> BonusTypes { get; set; }

    public DbSet<PayrollPeriod> PayrollPeriods { get; set; }

    public DbSet<Timesheet> Timesheets { get; set; }

    public DbSet<Department> Departments { get; set; }
    public DbSet<ExpenseType> ExpenseTypes { get; set; }
    public DbSet<ExpenseTypeHistory> ExpenseTypeHistories { get; set; }

    public DbSet<Contract> Contracts { get; set; }

    public DbSet<Account> Accounts { get; set; }
    public DbSet<BalanceEntry> AccountBalances { get; set; }
    public DbSet<AccountingRegulation> AccountingRegulations { get; set; }
    public DbSet<Balancesheet> Balancesheets { get; set; }
    public DbSet<UserInputBalancesheet> UserInputBalancesheets { get; set; }
    public DbSet<FinancialReportWork> FinancialReportWorks { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasSequence<int>(name: "CommonSeq", schema: "dbo")
                    .StartsAt(1)
                    .IncrementsBy(1);

        //modelBuilder.Entity<BalanceSheetDetail>()
        //            .Property(b => b.Id)
        //            .HasDefaultValueSql("NEXT VALUE FOR dbo.CommonSeq");

        //modelBuilder.Entity<BalanceSheet>()
        //            .HasMany<BalanceSheetDetail>(b => b.Details)
        //            .WithOne(b => b.BalanceSheet)
        //            .HasForeignKey("BlId").OnDelete(DeleteBehavior.Cascade);

        //modelBuilder.Entity<ImportedBalanceSheet>()
        //            .HasMany<ImportedBalanceSheetDetail>(b => b.Details)
        //            .WithOne(b => b.ImportedBalanceSheet)
        //            .HasForeignKey("BlId").OnDelete(DeleteBehavior.Cascade);

        //modelBuilder.Entity<BalanceSheet>()
        //            .HasOne<ImportedBalanceSheet>(b => b.ImportedBalanceSheet)
        //            .WithOne(ib => ib.BalanceSheet)
        //            .HasForeignKey<ImportedBalanceSheet>(i => i.BalanceSheetId)
        //            .OnDelete(DeleteBehavior.Cascade);

        //modelBuilder.Entity<Account>()
        //            .Property(a => a.Id)
        //            .ValueGeneratedNever();

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
                        mp.MenuId,
                        mp.PermissionId
                    }); // Khóa chính của bảng MenuPermission là MenuId và PermissionId

        modelBuilder.Entity<MenuPermission>()
                    .HasOne(mp => mp.MenuItem) // Một MenuPermission thuộc về một Menu
                    .WithMany(m => m.MenuPermissions) // Một Menu có nhiều MenuPermission
                    .HasForeignKey(mp => mp.MenuId); // Khóa ngoại là MenuId

        modelBuilder.Entity<MenuPermission>()
                    .HasOne(mp => mp.Permission) // Một MenuPermission thuộc về một Permission
                    .WithMany(p => p.MenuPermissions) // Một Permission có nhiều MenuPermission
                    .HasForeignKey(mp => mp.PermissionId);

        modelBuilder.Entity<Employee>(en =>
        {
            en.HasMany(e => e.Dependents) // Một Employee có nhiều Dependent
              .WithOne(d => d.Employee)
              .OnDelete(DeleteBehavior.Cascade) // Xóa
              .HasForeignKey(d => d.EmployeeId);

            en.HasMany(e => e.Salaries)
              .WithOne(s => s.Employee)
              .OnDelete(DeleteBehavior.Cascade)
              .HasForeignKey(s => s.EmployeeId);

            en.HasMany(e => e.Allowances)
              .WithOne(a => a.Employee)
              .OnDelete(DeleteBehavior.Cascade)
              .HasForeignKey(s => s.EmployeeId);
        });

        modelBuilder.Entity<Allowance>(en =>
        {
            en.HasOne(al => al.AllowanceType)
              .WithMany(at => at.Allowances)
              .HasForeignKey(al => al.AllowanceTypeId)
              .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Timesheet>(e =>
        {
            e.HasOne(s => s.PayrollPeriod)
             .WithMany(p => p.Timesheets)
             .HasForeignKey(s => s.PayrollPeriodId)
             .OnDelete(DeleteBehavior.NoAction);

            e.HasOne(s => s.Employee)
             .WithMany(em => em.Timesheets)
             .HasForeignKey(s => s.EmployeeId)
             .OnDelete(DeleteBehavior.NoAction);

            e.Property(s => s.Id).HasColumnType("CHAR(26)");
        });

        modelBuilder.Entity<Bonus>(e =>
        {
            e.HasOne(b => b.BonusType)
             .WithMany(t => t.Bonus)
             .HasForeignKey(b => b.BonusTypeId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(b => b.Employee)
             .WithMany(em => em.Bonus)
             .HasForeignKey(b => b.EmployeeId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Department>(e =>
        {
            e.Property(d => d.Id).HasColumnType("CHAR(26)");
        });

        modelBuilder.Entity<ExpenseTypeHistory>(e =>
        {
            e.Property(eh => eh.Id).HasColumnType("CHAR(26)");

            e.HasOne(eh => eh.ExpenseType)
             .WithMany(et => et.ExpenseTypeHistories)
             .HasForeignKey(eh => eh.ExpenseTypeId)
             .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<InvoiceServiceToken>(en =>
        {
            en.HasIndex(i => i.TaxId);
        });

        modelBuilder.Entity<FinancialReportWork>(en =>
        {
            en.HasOne(e => e.UserInput)
              .WithOne(i => i.FinancialReportWork).OnDelete(DeleteBehavior.Cascade);
            en.HasOne(e => e.Balancesheet)
              .WithOne(b => b.FinancialReportWork).OnDelete(DeleteBehavior.Cascade);
            en.HasIndex(e => e.Year);
        });

        base.OnModelCreating(modelBuilder);
        modelBuilder.FinalizeModel();

        
    }
}