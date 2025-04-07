﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using WebApp.Core.Data;

#nullable disable

namespace WebApp.Core.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20250407070412_add_file_hash")]
    partial class add_file_hash
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.10")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.HasSequence<int>("CommonSeq", "dbo");

            modelBuilder.Entity("OrganizationUser", b =>
                {
                    b.Property<Guid>("OrganizationsId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("UsersId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("OrganizationsId", "UsersId");

                    b.HasIndex("UsersId");

                    b.ToTable("OrganizationUser");
                });

            modelBuilder.Entity("PermissionRole", b =>
                {
                    b.Property<int>("PermissionsId")
                        .HasColumnType("int");

                    b.Property<int>("RolesId")
                        .HasColumnType("int");

                    b.HasKey("PermissionsId", "RolesId");

                    b.HasIndex("RolesId");

                    b.ToTable("PermissionRole");
                });

            modelBuilder.Entity("RoleUser", b =>
                {
                    b.Property<int>("RolesId")
                        .HasColumnType("int");

                    b.Property<Guid>("UsersId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("RolesId", "UsersId");

                    b.HasIndex("UsersId");

                    b.ToTable("RoleUser");
                });

            modelBuilder.Entity("WebApp.Core.DomainEntities.Accounting.Account", b =>
                {
                    b.Property<int>("Id")
                        .HasColumnType("int");

                    b.Property<string>("AccountNumber")
                        .IsRequired()
                        .HasMaxLength(10)
                        .HasColumnType("nvarchar(10)");

                    b.Property<string>("B01NV")
                        .HasMaxLength(5)
                        .HasColumnType("nvarchar(5)");

                    b.Property<string>("B01TS")
                        .HasMaxLength(5)
                        .HasColumnType("nvarchar(5)");

                    b.Property<string>("B02")
                        .HasMaxLength(5)
                        .HasColumnType("nvarchar(5)");

                    b.Property<bool>("Deleted")
                        .HasColumnType("bit");

                    b.Property<int>("Grade")
                        .HasColumnType("int");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("nvarchar(255)");

                    b.Property<int?>("Parent")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("AccountNumber")
                        .IsUnique();

                    b.ToTable("Accounts");
                });

            modelBuilder.Entity("WebApp.Core.DomainEntities.Accounting.BalanceSheet", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("CreateAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("CreateBy")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("Deleted")
                        .HasColumnType("bit");

                    b.Property<DateTime>("From")
                        .HasColumnType("datetime2");

                    b.Property<DateTime>("IssueDate")
                        .HasColumnType("datetime2");

                    b.Property<DateTime>("LastUpdateAt")
                        .HasColumnType("datetime2");

                    b.Property<Guid?>("OrganizationId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime>("To")
                        .HasColumnType("datetime2");

                    b.Property<int>("Version")
                        .HasColumnType("int");

                    b.Property<int>("Year")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("OrganizationId");

                    b.ToTable("BalanceSheets");
                });

            modelBuilder.Entity("WebApp.Core.DomainEntities.Accounting.BalanceSheetDetail", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasDefaultValueSql("NEXT VALUE FOR dbo.CommonSeq");

                    b.Property<string>("Account")
                        .HasMaxLength(10)
                        .HasColumnType("nvarchar(10)");

                    b.Property<decimal>("AriseCredit")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("AriseDebit")
                        .HasColumnType("decimal(18,2)");

                    b.Property<string>("B01NV")
                        .HasMaxLength(5)
                        .HasColumnType("nvarchar(5)");

                    b.Property<string>("B01TS")
                        .HasMaxLength(5)
                        .HasColumnType("nvarchar(5)");

                    b.Property<string>("B02")
                        .HasMaxLength(5)
                        .HasColumnType("nvarchar(5)");

                    b.Property<int?>("BlId")
                        .HasColumnType("int");

                    b.Property<decimal>("CloseCredit")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("CloseDebit")
                        .HasColumnType("decimal(18,2)");

                    b.Property<bool>("Deleted")
                        .HasColumnType("bit");

                    b.Property<decimal>("OpenCredit")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("OpenDebit")
                        .HasColumnType("decimal(18,2)");

                    b.Property<string>("Parent")
                        .HasMaxLength(5)
                        .HasColumnType("nvarchar(5)");

                    b.HasKey("Id");

                    b.HasIndex("BlId");

                    b.ToTable("BalanceSheetDetails");
                });

            modelBuilder.Entity("WebApp.Core.DomainEntities.Accounting.BalanceSheetEntry", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("AccountId")
                        .HasColumnType("int");

                    b.Property<decimal>("AriseCredit")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("AriseDebit")
                        .HasColumnType("decimal(18,2)");

                    b.Property<int>("BalanceSheetId")
                        .HasColumnType("int");

                    b.Property<decimal>("CloseCredit")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("CloseDebit")
                        .HasColumnType("decimal(18,2)");

                    b.Property<DateTime>("CreateAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("CreateBy")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("Deleted")
                        .HasColumnType("bit");

                    b.Property<DateTime>("LastUpdateAt")
                        .HasColumnType("datetime2");

                    b.Property<decimal>("OpenCredit")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("OpenDebit")
                        .HasColumnType("decimal(18,2)");

                    b.HasKey("Id");

                    b.HasIndex("AccountId");

                    b.HasIndex("BalanceSheetId");

                    b.ToTable("BalanceSheetEntry");
                });

            modelBuilder.Entity("WebApp.Core.DomainEntities.Accounting.ImportedBalanceSheet", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int?>("BalanceSheetId")
                        .HasColumnType("int");

                    b.Property<DateTime>("CreateAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("CreateBy")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("Deleted")
                        .HasColumnType("bit");

                    b.Property<bool>("IsValid")
                        .HasColumnType("bit");

                    b.Property<DateTime>("LastUpdateAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("Name")
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<Guid?>("OrganizationId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<decimal>("SumAriseCredit")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("SumAriseDebit")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("SumCloseCredit")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("SumCloseDebit")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("SumOpenCredit")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("SumOpenDebit")
                        .HasColumnType("decimal(18,2)");

                    b.Property<int?>("Year")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("BalanceSheetId")
                        .IsUnique()
                        .HasFilter("[BalanceSheetId] IS NOT NULL");

                    b.HasIndex("CreateAt");

                    b.HasIndex("OrganizationId");

                    b.ToTable("ImportedBalanceSheets");
                });

            modelBuilder.Entity("WebApp.Core.DomainEntities.Accounting.ImportedBalanceSheetDetail", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Account")
                        .HasMaxLength(10)
                        .HasColumnType("nvarchar(10)");

                    b.Property<decimal>("AriseCredit")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("AriseDebit")
                        .HasColumnType("decimal(18,2)");

                    b.Property<int>("BlId")
                        .HasColumnType("int");

                    b.Property<decimal>("CloseCredit")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("CloseDebit")
                        .HasColumnType("decimal(18,2)");

                    b.Property<bool>("Deleted")
                        .HasColumnType("bit");

                    b.Property<bool?>("IsValid")
                        .HasColumnType("bit");

                    b.Property<string>("Note")
                        .HasMaxLength(255)
                        .HasColumnType("nvarchar(255)");

                    b.Property<decimal>("OpenCredit")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("OpenDebit")
                        .HasColumnType("decimal(18,2)");

                    b.HasKey("Id");

                    b.HasIndex("Account");

                    b.HasIndex("BlId");

                    b.ToTable("ImportedBalanceSheetDetails");
                });

            modelBuilder.Entity("WebApp.Core.DomainEntities.Accounting.SyncInvoiceHistory", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<long>("Id"));

                    b.Property<DateTime>("CreateAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("CreateBy")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("Deleted")
                        .HasColumnType("bit");

                    b.Property<DateTime>("FromDate")
                        .HasColumnType("datetime2");

                    b.Property<DateTime>("LastUpdateAt")
                        .HasColumnType("datetime2");

                    b.Property<int>("SyncType")
                        .HasColumnType("int");

                    b.Property<string>("TaxId")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("nvarchar(20)");

                    b.Property<DateTime>("ToDateTime")
                        .HasColumnType("datetime2");

                    b.Property<int>("TotalFound")
                        .HasColumnType("int");

                    b.Property<int>("TotalSuccess")
                        .HasColumnType("int");

                    b.Property<string>("Username")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.HasKey("Id");

                    b.HasIndex("TaxId");

                    b.ToTable("SyncInvoiceHistories");
                });

            modelBuilder.Entity("WebApp.Core.DomainEntities.District", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("AlterName")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("nvarchar(255)");

                    b.Property<string>("Code")
                        .IsRequired()
                        .HasMaxLength(10)
                        .HasColumnType("nvarchar(10)");

                    b.Property<bool>("Deleted")
                        .HasColumnType("bit");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("nvarchar(255)");

                    b.Property<int?>("ProvinceId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("ProvinceId");

                    b.ToTable("RegionDistrict");
                });

            modelBuilder.Entity("WebApp.Core.DomainEntities.OrgDocument", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("AdjustmentType")
                        .HasMaxLength(1)
                        .HasColumnType("nvarchar(1)");

                    b.Property<DateTime>("CreateAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("CreateBy")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("Deleted")
                        .HasColumnType("bit");

                    b.Property<DateTime?>("DocumentDate")
                        .HasMaxLength(10)
                        .HasColumnType("datetime2");

                    b.Property<string>("DocumentName")
                        .HasMaxLength(255)
                        .HasColumnType("nvarchar(255)");

                    b.Property<int>("DocumentType")
                        .HasColumnType("int");

                    b.Property<string>("FileName")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("nvarchar(255)");

                    b.Property<string>("FilePath")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("nvarchar(255)");

                    b.Property<long>("FileSize")
                        .HasColumnType("bigint");

                    b.Property<string>("Hash")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("nvarchar(255)");

                    b.Property<DateTime>("LastUpdateAt")
                        .HasColumnType("datetime2");

                    b.Property<int?>("NumberOfAdjustment")
                        .HasColumnType("int");

                    b.Property<Guid>("OrganizationId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Period")
                        .HasMaxLength(10)
                        .HasColumnType("nvarchar(10)");

                    b.Property<string>("PeriodType")
                        .HasMaxLength(1)
                        .HasColumnType("nvarchar(1)");

                    b.Property<DateTime>("UploadTime")
                        .HasColumnType("datetime2");

                    b.HasKey("Id");

                    b.HasIndex("OrganizationId");

                    b.ToTable("Documents");
                });

            modelBuilder.Entity("WebApp.Core.DomainEntities.Organization", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Address")
                        .HasMaxLength(1000)
                        .HasColumnType("nvarchar(1000)");

                    b.Property<string>("ContactAddress")
                        .HasMaxLength(1000)
                        .HasColumnType("nvarchar(1000)");

                    b.Property<DateTime>("CreateAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("CreateBy")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("Deleted")
                        .HasColumnType("bit");

                    b.Property<int?>("DistrictId")
                        .HasColumnType("int");

                    b.Property<string>("Emails")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("FiscalYearFistDate")
                        .HasMaxLength(5)
                        .HasColumnType("nvarchar(5)");

                    b.Property<string>("FullName")
                        .IsRequired()
                        .HasMaxLength(500)
                        .HasColumnType("nvarchar(500)");

                    b.Property<string>("InvoicePwd")
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<DateTime>("LastUpdateAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("Phones")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("PinCode")
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<string>("ShortName")
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<string>("TaxId")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("nvarchar(20)");

                    b.Property<string>("TaxIdPwd")
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<int?>("TaxOfficeId")
                        .HasColumnType("int");

                    b.Property<string>("UnsignName")
                        .IsRequired()
                        .HasMaxLength(500)
                        .HasColumnType("nvarchar(500)");

                    b.HasKey("Id");

                    b.HasIndex("DistrictId");

                    b.HasIndex("TaxId");

                    b.HasIndex("TaxOfficeId");

                    b.HasIndex("UnsignName");

                    b.ToTable("Organizations");
                });

            modelBuilder.Entity("WebApp.Core.DomainEntities.OrganizationInfo", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Address")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("nvarchar(255)");

                    b.Property<DateTime>("CreateAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("CreateBy")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("Deleted")
                        .HasColumnType("bit");

                    b.Property<int>("District")
                        .HasColumnType("int");

                    b.Property<string>("FullName")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("nvarchar(255)");

                    b.Property<bool>("IsCurrent")
                        .HasColumnType("bit");

                    b.Property<DateTime>("LastUpdateAt")
                        .HasColumnType("datetime2");

                    b.Property<Guid>("Organization")
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("TaxOfficeId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.ToTable("OrganizationInfos");
                });

            modelBuilder.Entity("WebApp.Core.DomainEntities.Permission", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<bool>("Deleted")
                        .HasColumnType("bit");

                    b.Property<string>("Description")
                        .HasMaxLength(255)
                        .HasColumnType("nvarchar(255)");

                    b.Property<string>("FrontEndAccess")
                        .HasMaxLength(255)
                        .HasColumnType("nvarchar(255)");

                    b.Property<string>("PermissionName")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("nvarchar(255)");

                    b.HasKey("Id");

                    b.ToTable("Permissions");
                });

            modelBuilder.Entity("WebApp.Core.DomainEntities.Province", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("AlterName")
                        .HasMaxLength(255)
                        .HasColumnType("nvarchar(255)");

                    b.Property<string>("Code")
                        .IsRequired()
                        .HasMaxLength(10)
                        .HasColumnType("nvarchar(10)");

                    b.Property<bool>("Deleted")
                        .HasColumnType("bit");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("nvarchar(255)");

                    b.HasKey("Id");

                    b.ToTable("RegionProvince");
                });

            modelBuilder.Entity("WebApp.Core.DomainEntities.RiskCompany", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("CreateAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("CreateBy")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("Deleted")
                        .HasColumnType("bit");

                    b.Property<DateTime>("LastUpdateAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("nvarchar(255)");

                    b.Property<string>("TaxId")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("nvarchar(255)");

                    b.Property<string>("TaxOffice")
                        .HasMaxLength(255)
                        .HasColumnType("nvarchar(255)");

                    b.HasKey("Id");

                    b.HasIndex("TaxId")
                        .IsUnique();

                    b.ToTable("RiskCompanies");
                });

            modelBuilder.Entity("WebApp.Core.DomainEntities.Role", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<bool>("Deleted")
                        .HasColumnType("bit");

                    b.Property<string>("Description")
                        .HasMaxLength(255)
                        .HasColumnType("nvarchar(255)");

                    b.Property<string>("RoleName")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("nvarchar(255)");

                    b.HasKey("Id");

                    b.ToTable("Roles");
                });

            modelBuilder.Entity("WebApp.Core.DomainEntities.TaxOffice", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Code")
                        .IsRequired()
                        .HasMaxLength(10)
                        .HasColumnType("nvarchar(10)");

                    b.Property<bool>("Deleted")
                        .HasColumnType("bit");

                    b.Property<string>("FullName")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("nvarchar(255)");

                    b.Property<int?>("ParentId")
                        .HasColumnType("int");

                    b.Property<int?>("ProvinceId")
                        .HasColumnType("int");

                    b.Property<string>("ShortName")
                        .HasMaxLength(20)
                        .HasColumnType("nvarchar(20)");

                    b.HasKey("Id");

                    b.HasIndex("Code");

                    b.HasIndex("ProvinceId");

                    b.ToTable("TaxOffices");
                });

            modelBuilder.Entity("WebApp.Core.DomainEntities.User", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<bool>("Deleted")
                        .HasColumnType("bit");

                    b.Property<bool>("Locked")
                        .HasColumnType("bit");

                    b.Property<int>("LogInFailedCount")
                        .HasColumnType("int");

                    b.Property<string>("Password")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("nvarchar(255)");

                    b.Property<string>("Username")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("nvarchar(255)");

                    b.HasKey("Id");

                    b.HasIndex("Username");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("OrganizationUser", b =>
                {
                    b.HasOne("WebApp.Core.DomainEntities.Organization", null)
                        .WithMany()
                        .HasForeignKey("OrganizationsId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("WebApp.Core.DomainEntities.User", null)
                        .WithMany()
                        .HasForeignKey("UsersId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("PermissionRole", b =>
                {
                    b.HasOne("WebApp.Core.DomainEntities.Permission", null)
                        .WithMany()
                        .HasForeignKey("PermissionsId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("WebApp.Core.DomainEntities.Role", null)
                        .WithMany()
                        .HasForeignKey("RolesId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("RoleUser", b =>
                {
                    b.HasOne("WebApp.Core.DomainEntities.Role", null)
                        .WithMany()
                        .HasForeignKey("RolesId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("WebApp.Core.DomainEntities.User", null)
                        .WithMany()
                        .HasForeignKey("UsersId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("WebApp.Core.DomainEntities.Accounting.BalanceSheet", b =>
                {
                    b.HasOne("WebApp.Core.DomainEntities.Organization", "Organization")
                        .WithMany()
                        .HasForeignKey("OrganizationId");

                    b.Navigation("Organization");
                });

            modelBuilder.Entity("WebApp.Core.DomainEntities.Accounting.BalanceSheetDetail", b =>
                {
                    b.HasOne("WebApp.Core.DomainEntities.Accounting.BalanceSheet", "BalanceSheet")
                        .WithMany("Details")
                        .HasForeignKey("BlId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.Navigation("BalanceSheet");
                });

            modelBuilder.Entity("WebApp.Core.DomainEntities.Accounting.BalanceSheetEntry", b =>
                {
                    b.HasOne("WebApp.Core.DomainEntities.Accounting.Account", "Account")
                        .WithMany()
                        .HasForeignKey("AccountId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("WebApp.Core.DomainEntities.Accounting.BalanceSheet", null)
                        .WithMany("Entries")
                        .HasForeignKey("BalanceSheetId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Account");
                });

            modelBuilder.Entity("WebApp.Core.DomainEntities.Accounting.ImportedBalanceSheet", b =>
                {
                    b.HasOne("WebApp.Core.DomainEntities.Accounting.BalanceSheet", "BalanceSheet")
                        .WithOne("ImportedBalanceSheet")
                        .HasForeignKey("WebApp.Core.DomainEntities.Accounting.ImportedBalanceSheet", "BalanceSheetId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("WebApp.Core.DomainEntities.Organization", "Organization")
                        .WithMany()
                        .HasForeignKey("OrganizationId");

                    b.Navigation("BalanceSheet");

                    b.Navigation("Organization");
                });

            modelBuilder.Entity("WebApp.Core.DomainEntities.Accounting.ImportedBalanceSheetDetail", b =>
                {
                    b.HasOne("WebApp.Core.DomainEntities.Accounting.ImportedBalanceSheet", "ImportedBalanceSheet")
                        .WithMany("Details")
                        .HasForeignKey("BlId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("ImportedBalanceSheet");
                });

            modelBuilder.Entity("WebApp.Core.DomainEntities.District", b =>
                {
                    b.HasOne("WebApp.Core.DomainEntities.Province", "Province")
                        .WithMany("Districts")
                        .HasForeignKey("ProvinceId");

                    b.Navigation("Province");
                });

            modelBuilder.Entity("WebApp.Core.DomainEntities.OrgDocument", b =>
                {
                    b.HasOne("WebApp.Core.DomainEntities.Organization", "Organization")
                        .WithMany()
                        .HasForeignKey("OrganizationId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Organization");
                });

            modelBuilder.Entity("WebApp.Core.DomainEntities.Organization", b =>
                {
                    b.HasOne("WebApp.Core.DomainEntities.District", "District")
                        .WithMany()
                        .HasForeignKey("DistrictId");

                    b.HasOne("WebApp.Core.DomainEntities.TaxOffice", "TaxOffice")
                        .WithMany()
                        .HasForeignKey("TaxOfficeId");

                    b.Navigation("District");

                    b.Navigation("TaxOffice");
                });

            modelBuilder.Entity("WebApp.Core.DomainEntities.TaxOffice", b =>
                {
                    b.HasOne("WebApp.Core.DomainEntities.Province", "Province")
                        .WithMany("TaxOffices")
                        .HasForeignKey("ProvinceId");

                    b.Navigation("Province");
                });

            modelBuilder.Entity("WebApp.Core.DomainEntities.Accounting.BalanceSheet", b =>
                {
                    b.Navigation("Details");

                    b.Navigation("Entries");

                    b.Navigation("ImportedBalanceSheet");
                });

            modelBuilder.Entity("WebApp.Core.DomainEntities.Accounting.ImportedBalanceSheet", b =>
                {
                    b.Navigation("Details");
                });

            modelBuilder.Entity("WebApp.Core.DomainEntities.Province", b =>
                {
                    b.Navigation("Districts");

                    b.Navigation("TaxOffices");
                });
#pragma warning restore 612, 618
        }
    }
}
