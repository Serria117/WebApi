using System.Dynamic;
using System.Reflection;
using System.Text.Json;
using Microsoft.IdentityModel.Tokens;
using WebApp.Core.DomainEntities;
using WebApp.Core.DomainEntities.Accounting;
using WebApp.Core.DomainEntities.Accounting.FinancialStatement;
using WebApp.Core.DomainEntities.Payroll;
using WebApp.Enums;
using WebApp.Repositories;
using WebApp.Services.BalanceSheetService.Dto;
using WebApp.Services.CommonService;
using WebApp.Services.OrganizationService.Dto;
using WebApp.Services.PayrollService.Dto;
using WebApp.Services.RegionService.Dto;
using WebApp.Services.TemplateServices.Dto;
using WebApp.Services.UserService.Dto;
using X.PagedList;

namespace WebApp.Services.Mappers;

public static class DataObjectMapExtension
{
    /// <summary>
    /// Maps a paged list of entities to a paged list of another type using the provided mapping function.
    /// </summary>
    /// <param name="entities">The original paged list of entities to be mapped.</param>
    /// <param name="mapFunc">The mapping function to convert each entity into the desired type.</param>
    /// <typeparam name="TEntity">The type of the entities in the input paged list.</typeparam>
    /// <typeparam name="TDto">The target type of the mapped entities.</typeparam>
    /// <returns>A new paged list containing the mapped entities of the target type.</returns>
    public static IPagedList<TDto> MapPagedList<TEntity, TDto>(this IPagedList<TEntity> entities,
                                                               Func<TEntity, TDto> mapFunc)
    {
        var dtoList = entities.Select(mapFunc);
        return new StaticPagedList<TDto>(subset: dtoList, metaData: entities);
    }

    /// <summary>
    /// Projects a paged list of entities into a paged list of dynamic objects based on the specified fields.
    /// </summary>
    /// <param name="entities">The original paged list of entities to be projected.</param>
    /// <param name="fields">An array of field names to include in the projection. Each dynamic object will contain only these fields.</param>
    /// <typeparam name="TEntity">The type of the entities contained in the original paged list.</typeparam>
    /// <returns>A new paged list containing dynamic objects (ExpandoObject) with the specified fields, preserving the metadata of the original paged list.</returns>
    public static IPagedList<ExpandoObject> ProjectPagedList<TEntity>(this IPagedList<TEntity> entities,
                                                                      string[] fields)
        where TEntity : class // Ràng buộc TEntity là class
    {
        // Xử lý trường hợp fields rỗng hoặc null: trả về danh sách rỗng hoặc xử lý tùy ý
        if (fields.Length == 0)
        {
            // Tùy chọn: có thể trả về StaticPagedList với danh sách ExpandoObject rỗng
            var emptyDynamicList = new List<ExpandoObject>();
            return new StaticPagedList<ExpandoObject>(
                subset: emptyDynamicList,
                metaData: entities // Vẫn giữ metadata từ PagedList gốc
            );
            // Hoặc ném ArgumentException nếu việc yêu cầu 0 trường là không hợp lệ
            // throw new ArgumentException("Fields array cannot be null or empty.", nameof(fields));
        }

        var selectedFieldsList = new List<ExpandoObject>();
        var itemType = typeof(TEntity);

        foreach (var item in entities) // entities là IEnumerable<TEntity>
        {
            var expando = new ExpandoObject() as IDictionary<string, object>;

            foreach (var fieldName in fields)
            {
                // Sử dụng BindingFlags để tìm kiếm thuộc tính không phân biệt hoa thường
                var propertyInfo =
                    itemType.GetProperty(
                        fieldName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

                if (propertyInfo != null)
                {
                    try
                    {
                        var value = propertyInfo.GetValue(item);
                        var camelCaseFieldName = JsonNamingPolicy.CamelCase.ConvertName(propertyInfo.Name);
                        expando[camelCaseFieldName] = value;
                    }
                    catch (Exception ex)
                    {
                        // Xử lý lỗi khi không thể lấy giá trị thuộc tính (ví dụ: thuộc tính chỉ ghi)
                        // hoặc log lỗi. Trong ví dụ này, ta bỏ qua thuộc tính này.
                        //Console.WriteLine($"Could not get value for property {fieldName}: {ex.Message}");
                    }
                }
                // Tùy chọn: Nếu tên trường không tồn tại, bạn có thể thêm nó với giá trị null
                // else
                // {
                //     expando[fieldName] = null;
                // }
            }

            selectedFieldsList.Add((ExpandoObject)expando!);
        }

        // Tạo StaticPagedList<ExpandoObject> từ danh sách các đối tượng động
        // và metadata của PagedList gốc.
        var pagedResult = new StaticPagedList<ExpandoObject>(
            subset: selectedFieldsList, // Danh sách các đối tượng động
            metaData: entities // Thông tin phân trang từ kết quả gốc
        );

        return pagedResult;
    }

    /// <summary>
    /// Asynchronously maps a paged list of entities to a paged list of another type using the provided asynchronous mapping function.
    /// </summary>
    /// <param name="entities">The original paged list of entities to be mapped.</param>
    /// <param name="mapFunc">The asynchronous mapping function to convert each entity into the desired type.</param>
    /// <typeparam name="TEntity">The type of the entities in the input paged list.</typeparam>
    /// <typeparam name="TDto">The target type of the mapped entities.</typeparam>
    /// <returns>A task representing the asynchronous operation. The task result contains a new paged list of the mapped entities of the target type.</returns>
    public static async Task<IPagedList<TDto>> MapPagedListAsync<TEntity, TDto>(this IPagedList<TEntity> entities,
        Func<TEntity, Task<TDto>> mapFunc)
    {
        var dtoList = await Task.WhenAll(entities.Select(mapFunc));
        return new StaticPagedList<TDto>(subset: dtoList, metaData: entities);
    }

    /// <summary>
    /// Maps a collection of entities to a collection of another type using the provided mapping function.
    /// </summary>
    /// <param name="entities">The original collection of entities to be mapped.</param>
    /// <param name="mapFunc">The mapping function to convert each entity into the desired type.</param>
    /// <typeparam name="TEntity">The type of the entities in the input collection.</typeparam>
    /// <typeparam name="TDto">The target type of the mapped entities.</typeparam>
    /// <returns>A new collection containing the mapped entities of the target type.</returns>
    public static IEnumerable<TDto> MapCollection<TEntity, TDto>(this IEnumerable<TEntity> entities,
                                                                 Func<TEntity, TDto> mapFunc)
    {
        return entities.Select(mapFunc);
    }

    public static ExpandoObject ProjectToDisplay<TEntity>(this TEntity entity, List<string>? fields = null)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));

        var expando = new ExpandoObject() as IDictionary<string, object>;
        var type = typeof(TEntity);

        // If fields is null or empty, map all public instance properties
        var propertyInfos = (fields is null || fields.Count == 0)
            ? type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            : fields.Select(f => type.GetProperty(
                                f, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance))
                    .Where(p => p != null)
                    .ToArray();

        foreach (var prop in propertyInfos)
        {
            try
            {
                var value = prop.GetValue(entity);
                var camelCaseFieldName = JsonNamingPolicy.CamelCase.ConvertName(prop.Name);
                expando[camelCaseFieldName] = value;
            }
            catch
            {
                // Ignore property if cannot get value
            }
        }

        return (ExpandoObject)expando!;
    }

    #region Organization

    public static OrganizationDisplayDto ToDisplayDto(this Organization o)
    {
        return new OrganizationDisplayDto
        {
            Id = o.Id,
            FullName = o.FullName,
            Address = o.Address,
            District = o.District is null
                ? null
                : new DistrictDisplayDto
                {
                    Id = o.District.Id,
                    Name = o.District.Name,
                    Code = o.District.Code,
                    AlterName = o.District.AlterName,
                    ProvinceId = o.District.Province?.Id,
                    ProvinceName = o.District.Province?.Name,
                },
            CapitalOwnershipType = o.CapitalOwnershipType switch
            {
                CapitalOwnershipType.PrivateCompany => CapitalOwner.PrivateCompany,
                CapitalOwnershipType.LimitedCompany => CapitalOwner.LimitedCompany,
                CapitalOwnershipType.LimitedCompanySingleOwner => CapitalOwner.LimitedCompanySingleOwner,
                CapitalOwnershipType.JointStockCompany => CapitalOwner.JointStockCompany,
                CapitalOwnershipType.HouseholdBusiness => CapitalOwner.HouseholdBusiness,
                CapitalOwnershipType.Cooperative => CapitalOwner.Cooperative,
                CapitalOwnershipType.Fdi => CapitalOwner.Fdi,
                CapitalOwnershipType.Other => CapitalOwner.Other,
                _ => null
            },
            Emails = o.Emails,
            Phones = o.Phones,
            ContactAddress = o.ContactAddress,
            TaxId = o.TaxId,
            TaxOffice = o.TaxOffice2 is null
                ? null
                : new TaxOfficeDisplayDto
                {
                    Id = o.TaxOffice2.Id,
                    FullName = o.TaxOffice2.FullName,
                    ShortName = o.TaxOffice2.ShortName,
                    Code = o.TaxOffice2.Code,
                    //ParentId = o.TaxOffice.ParentId,
                },
            ShortName = o.ShortName,
            InvoicePwd = o.InvoicePwd,
            PinCode = o.PinCode,
            TaxIdPwd = o.TaxIdPwd,
            CreateAt = o.CreateAt,
            CreateBy = o.CreateBy,
            LastUpdateAt = o.LastUpdateAt,
            FiscalYearFirstDate = o.FiscalYearFistDate,
            TypeOfVatPeriod = o.TypeOfVatPeriod,
            Representative = o.Representative,
            OrganizationLoginInfos = o.OrganizationLoginInfos
                                      .Select(x => new OrganizationLoginInfoDto
                                      {
                                          Id = x.Id,
                                          AccountName = x.AccountName,
                                          Provider = x.Provider,
                                          Password = x.Password,
                                          Url = x.Url,
                                          Username = x.Username,
                                      })
                                      .ToHashSet()
        };
    }

    public static Organization ToEntity(this OrganizationInputDto i)
    {
        return new Organization
        {
            FullName = i.FullName.RemoveSpace() ?? string.Empty,
            ShortName = i.ShortName.RemoveSpace(),
            Address = i.Address.RemoveSpace(),
            ContactAddress = i.ContactAddress.RemoveSpace(),
            Emails = [.. i.Emails.Select(x => x.RemoveSpace()!)],
            Phones = [.. i.Phones.Select(x => x.RemoveSpace()!)],
            InvoicePwd = string.IsNullOrEmpty(i.InvoicePwd) ? null : i.InvoicePwd,
            PinCode = string.IsNullOrEmpty(i.PinCode) ? null : i.PinCode,
            TaxId = i.TaxId.RemoveSpace()!,
            UnsignName = i.FullName.RemoveSpace()!.UnSign(),
            TaxIdPwd = string.IsNullOrEmpty(i.TaxIdPwd) ? null : i.TaxIdPwd,
            TypeOfVatPeriod = i.TypeOfVatPeriod.RemoveSpace() ?? "Q",
            CapitalOwnershipType = (CapitalOwnershipType?)i.CapitalOwnershipType,
            Representative = i.Representative.RemoveSpace(),
            OrganizationLoginInfos =
            [
                .. i.OrganizationLoginInfos
                    .Select(x => new OrganizationLoginInfo
                    {
                        AccountName = x.AccountName,
                        Password = x.Password,
                        Provider = x.Provider,
                        Url = x.Url,
                        Username = x.Username
                    })
            ]
        };
    }

    public static void UpdateEntity(this OrganizationInputDto i, Organization o)
    {
        o.FullName = i.FullName.RemoveSpace() ?? o.FullName;
        o.ContactAddress = i.ContactAddress.RemoveSpace();
        o.ShortName = i.ShortName.RemoveSpace() ?? o.ShortName;
        o.Emails = i.Emails.IsNullOrEmpty() ? [] : i.Emails.Select(x => x.RemoveSpace()!).ToList();
        o.Address = i.Address.RemoveSpace();
        o.UnsignName = o.FullName.UnSign();
        o.Phones = i.Phones.IsNullOrEmpty() ? [] : i.Phones.Select(x => x.RemoveSpace()!).ToList();
        o.InvoicePwd = i.InvoicePwd.RemoveSpace();
        o.TaxIdPwd = i.TaxIdPwd.RemoveSpace();
        o.PinCode = i.PinCode.RemoveSpace();
        //o.TaxId = i.TaxId.RemoveSpace();
        o.TypeOfVatPeriod = i.TypeOfVatPeriod.RemoveSpace();
        o.Representative = i.Representative.RemoveSpace();
        o.CapitalOwnershipType = (CapitalOwnershipType?)i.CapitalOwnershipType;
        /*o.OrganizationLoginInfos = i.OrganizationLoginInfos
                                    .Select(x => new OrganizationLoginInfo
                                    {
                                        AccountName = x.AccountName,
                                        Password = x.Password,
                                        Provider = x.Provider,
                                        Url = x.Url,
                                        Username = x.Username
                                    })
                                    .ToHashSet();*/
    }

    #endregion

    #region Province

    public static ProvinceDisplayDto ToDisplayDto(this Province o)
    {
        return new ProvinceDisplayDto
        {
            Id = o.Id,
            Code = o.Code,
            AlterName = o.AlterName,
            Name = o.Name,
            Districts = o.Districts.MapCollection(x => x.ToDisplayDto()).ToHashSet(),
            TaxOffices = o.TaxOffices.MapCollection(x => x.ToDisplayDto()).ToHashSet(),
        };
    }

    public static Province ToEntity(this ProvinceCreateDto d)
    {
        return new Province
        {
            Code = d.Code,
            Name = d.Name,
            AlterName = d.AlterName
        };
    }

    public static void UpdateEntity(this ProvinceCreateDto d, Province o)
    {
        o.Name = d.Name.RemoveSpace() ?? o.Name;
        o.Code = d.Code.RemoveSpace() ?? o.Code;
        o.AlterName = d.AlterName.RemoveSpace() ?? o.AlterName;
    }

    #endregion

    #region District

    public static DistrictDisplayDto ToDisplayDto(this District o)
    {
        return new DistrictDisplayDto
        {
            Id = o.Id,
            Name = o.Name,
            AlterName = o.AlterName,
            Code = o.Code,
            ProvinceId = o.Province?.Id
        };
    }

    public static District ToEntity(this DistrictCreateDto d, IAppRepository<Province, int> provinceRepo)
    {
        return new District
        {
            Code = d.Code.RemoveSpace()!,
            Name = d.Name.RemoveSpace()!,
            AlterName = d.AlterName.RemoveSpace()!,
            Province = provinceRepo.Attach(d.ProvinceId),
        };
    }

    public static void UpdateEntity(this DistrictCreateDto i, District e)
    {
        e.Code = i.Code.RemoveSpace() ?? i.Code;
        e.Name = e.Name.RemoveSpace() ?? e.Name;
        e.AlterName = e.AlterName.RemoveSpace() ?? e.AlterName;
    }

    #endregion

    #region TaxOfice

    public static TaxOfficeDisplayDto ToDisplayDto(this TaxOffice o)
    {
        return new TaxOfficeDisplayDto
        {
            Id = o.Id,
            Code = o.Code,
            FullName = o.FullName,
            ShortName = o.ShortName,
            ParentId = o.ParentId,
            Parent = o.Parent == null
                ? null
                : new TaxOfficeDisplayDto
                {
                    Id = o.Parent.Id,
                    FullName = o.Parent.FullName
                },
            Children = o.Children?.MapCollection(x => new TaxOfficeDisplayDto
            {
                FullName = x.FullName,
                ShortName = x.ShortName,
                Code = x.Code,
                ParentId = x.ParentId,
                Id = x.Id
            }).ToList(),
            Province = o.Province?.Name,
        };
    }

    public static TaxOffice ToEntity(this TaxOfficeCreateDto d,
                                     IAppRepository<Province, int> provinceRepo)
    {
        return new TaxOffice
        {
            FullName = d.FullName.RemoveSpace()!,
            ShortName = d.ShortName.RemoveSpace()!,
            Code = d.Code.RemoveSpace()!,
            ParentId = d.ParentId,
            Province = d.ProvinceId != null
                ? provinceRepo.Attach(id: d.ProvinceId.Value)
                : null //TODO: check if parent exists or not
        };
    }

    public static void UpdateEntity(this TaxOfficeCreateDto i, TaxOffice e)
    {
        e.Code = i.Code.RemoveSpace() ?? i.Code;
        e.FullName = i.FullName.RemoveSpace() ?? e.FullName;
        e.ShortName = e.ShortName.RemoveSpace() ?? e.ShortName;
        e.ParentId = e.ParentId;
    }

    #endregion

    #region User, Role, Permission

    public static UserDisplayDto ToDisplayDto(this User u)
    {
        return new UserDisplayDto()
        {
            Id = u.Id,
            Username = u.Username,
            FullName = u.FullName,
            Email = u.Email,
            Roles = u.Roles.Select(x => new RoleDisplayDto
            {
                Id = x.Id,
                RoleName = x.RoleName
            }).ToHashSet(),
            Organizations = u.Organizations.Select(o => new OrganizationInUserDto()
            {
                Id = o.Id,
                FullName = o.FullName,
                TaxId = o.TaxId
            }).ToList(),
            Locked = u.Locked,
        };
    }

    public static User ToEntity(this UserInputDto d)
    {
        return new User
        {
            Username = d.Username.RemoveSpace()!,
            Email = d.Email.RemoveSpace() ?? null,
            FullName = d.FullName.RemoveSpace() ?? null,
            Password = d.Password.BCryptHash(),
            Locked = d.Locked ?? false,
        };
    }

    public static void UpdateEntity(this User u, User e)
    {
        e.Password = u.Password.BCryptHash();
        e.FullName = u.FullName.RemoveSpace() ?? null;
        e.Locked = u.Locked;
        e.Email = u.Email.RemoveSpace() ?? null;
        // Username is forbidden to change
    }

    public static RoleDisplayDto ToDisplayDto(this Role role)
    {
        return new RoleDisplayDto
        {
            Id = role.Id,
            RoleName = role.RoleName,
            Description = role.Description,
            Permissions = role.Permissions.MapCollection(x => x.ToDisplayDto()).ToHashSet(),
            Users = role.Users.Count == 0
                ? []
                : role.Users.Select(u => new UserInfoDto
                {
                    Username = u.Username,
                    Id = u.Id,
                }).ToHashSet(),
        };
    }

    public static Role ToEntity(this RoleInputDto d)
    {
        return new Role
        {
            RoleName = d.RoleName,
            Description = d.Description,
        };
    }

    public static void UpdateEntity(this RoleInputDto i, Role r)
    {
        r.Description = i.Description;
        r.RoleName = i.RoleName;
    }

    public static PermissionDisplayDto ToDisplayDto(this Permission permission)
    {
        return new PermissionDisplayDto
        {
            Id = permission.Id,
            PermissionName = permission.PermissionName,
            Description = permission.Description,
        };
    }

    #endregion

    #region PayrollPeriod

    public static PayrollPeriodDisplay ToDisplayDto(this PayrollPeriod p)
    {
        return new PayrollPeriodDisplay
        {
            Id = p.Id,
            Year = p.Year,
            OrganizationId = p.OrganizationId,
            Version = p.Version,
            StartDate = p.StartDate,
            EndDate = p.EndDate,
            Weekend = p.Weekend,
            TotalWorkDay = p.TotalWorkDay,
            Name = p.Name,
        };
    }

    #endregion

    #region Employee

    public static EmployeeDisplay ToDisplayDto(this Employee e)
    {
        return new EmployeeDisplay
        {
            Id = e.Id,
            Name = e.Name,
            Code = e.Code,
            JoinDate = e.JoinDate,
            EndDate = e.EndDate,
            PersonalId = e.PersonalId,
            TaxId = e.TaxId,
            Dependents = [.. e.Dependents.Select(d => d.ToDisplayDto())],
            Salaries = [.. e.Salaries.Select(s => s.ToDisplayDto())]
        };
    }

    public static Employee ToEntity(this EmployeeCreateDto d, Organization org)
    {
        return new Employee
        {
            Name = d.Name,
            Code = d.Code,
            TaxId = d.TaxId,
            PersonalId = d.PersonalId,
            JoinDate = d.JoinDate,
            EndDate = d.EndDate,
            Organization = org,
            OtherDocument = d.OtherDocument,
            OtherDocumentId = d.OtherDocumentId
        };
    }

    public static DependentDisplay ToDisplayDto(this Dependents d)
    {
        return new DependentDisplay
        {
            Id = d.Id,
            Name = d.Name,
            EffectiveDate = d.EffectiveDate,
            EndDate = d.EndDate,
            DateOfBirth = d.DateOfBirth,
            EmployeeId = d.EmployeeId,
            PersonalId = d.PersonalId,
            TaxId = d.TaxId,
            Relationship = d.Relationship,
            OtherDocument = d.OtherDocument,
            OtherDocumentId = d.OtherDocumentId
        };
    }

    public static Dependents ToEntity(this DependentCreateDto d)
    {
        return new Dependents
        {
            Name = d.Name,
            PersonalId = d.PersonalId,
            TaxId = d.TaxId,
            DateOfBirth = d.DateOfBirth,
            EffectiveDate = d.EffectiveDate,
            EndDate = d.EndDate,
            Relationship = d.Relationship,
            OtherDocument = d.OtherDocument,
            OtherDocumentId = d.OtherDocumentId
        };
    }

    public static SalaryDisplay ToDisplayDto(this Salary s)
    {
        return new SalaryDisplay
        {
            EffectiveDate = s.EffectiveDate,
            EndDate = s.EndDate,
            Id = s.Id,
            InsuranceSalaryValue = s.InsuranceSalaryValue,
            SalaryValue = s.SalaryValue,
        };
    }

    public static Allowance ToEntity(this AllowanceCreate a)
    {
        return new Allowance
        {
            Amount = a.Amount,
            AllowanceTypeId = a.AllowanceTypeId,
            EffectiveDate = a.EffectiveDate,
            EndDate = a.EndDate,
        };
    }

    public static AllowanceType ToEntity(this AllowanceTypeCreate a, Guid? orgId)
    {
        return new AllowanceType
        {
            Name = a.Name,
            Code = a.Code,
            IsInsurance = a.IsInsurance,
            IsTaxable = a.IsTaxable,
            OrganizationId = a.IsOrganization ? orgId : null,
            MaxAmount = a.MaxAmount,
            DefaultAmount = a.DefaultAmount,
            Unit = a.Unit?.RemoveSpace() ?? string.Empty,
        };
    }

    #endregion

    #region TEMPLATE

    public static TemplateDisplayDto ToDisplayDto(this Template entity)
    {
        return new TemplateDisplayDto
        {
            Name = entity.Name,
            Description = entity.Description,
            Order = entity.Order,
            TemplateFiles = [.. entity.TemplateFiles.Select(t => t.ToDisplayDto())]
        };
    }

    public static Template ToEntity(this TemplateCreateDto dto)
    {
        return new Template
        {
            Name = dto.Name,
            Description = dto.Description,
            Order = dto.Order,
        };
    }

    public static TemplateFileDisplayDto ToDisplayDto(this TemplateFile entity)
    {
        return new TemplateFileDisplayDto
        {
            Id = entity.Id,
            Version = entity.Version,
            FileName = entity.FileName,
            FilePath = entity.FilePath,
            VersionNote = entity.VersionNote,
            UploadTime = entity.UploadTime,
        };
    }

    public static TemplateFile ToEntity(this TemplateFileCreateDto dto)
    {
        return new TemplateFile
        {
            Version = dto.Version,
            VersionNote = dto.VersionNote
        };
    }

    #endregion

    #region Financial Report

    public static FinancialReportDisplayDto ToDisplayDto(this FinancialReportWork r)
    {
        return new FinancialReportDisplayDto
        {
            Id = r.Id,
            OrganizationId = r.OrganizationId,
            Name = r.Name,
            Regulation = r.Regulation,
            Note = r.Note,
            BeginDate = r.BeginDate,
            EndDate = r.EndDate,
            ReportDate = r.ReportDate,
            Year = r.Year,
            FirstFiscalDate = r.FirstFiscalDate,
            TaxAgencyCode = r.TaxAgencyCode,
            TaxAgencyName = r.TaxAgencyName,
            Status = r.Status,
            UserInput = r.UserInput == null
                ? null
                : new UserInputTrialDto()
                {
                    Id = r.UserInput.Id,
                    Entries = r.UserInput.Entries.Select(e => new UserInputEntryDto
                    {
                        Id = e.Id,
                        Name = e.Name,
                        AccountCode = e.AccountCode,
                        OpenCredit = e.OpenCredit,
                        OpenDebit = e.OpenDebit,
                        AriseCredit = e.AriseCredit,
                        AriseDebit = e.AriseDebit,
                        CloseCredit = e.CloseCredit,
                        CloseDebit = e.CloseDebit,
                        IsMatchRegulation = e.IsMatchRegulation,
                        IsUserInput = e.IsUserInput,
                        ParentCode = e.ParentCode,
                        NetBalanceValid = e.NetBalanceValid,
                        InvalidNetBalanceDifference = e.InvalidNetBalanceDifference
                    }).ToArray()
                },
            TrialBalance = r.TrialBalanceEntries.Select(e => new TrialBalanceEntryDto
            {
                Id = e.Id,
                Name = e.Name,
                AccountCode = e.AccountCode,
                OpenCredit = e.OpenCredit,
                OpenDebit = e.OpenDebit,
                AriseCredit = e.AriseCredit,
                AriseDebit = e.AriseDebit,
                CloseCredit = e.CloseCredit,
                CloseDebit = e.CloseDebit,
                IsMatchRegulation = e.IsMatchRegulation,
                IsUserInput = e.IsUserInput,
                ParentCode = e.ParentCode,
                NetBalanceValid = e.NetBalanceValid,
                InvalidNetBalanceDifference = e.InvalidNetBalanceDifference
            }).ToArray(),
            IncomeStatement = r.IncomeStatementEntries.Select(i => new IncomeStatementEntryDto
            {
                Id = i.Id,
                Name = i.Name,
                Code = i.Code,
                LastYear = i.LastYear,
                ThisYear = i.ThisYear
            }).ToArray(),
            BalanceSheet = r.BalanceSheetEntries.Select(b => new BalanceSheetEntryDto
            {
                Id = b.Id,
                Name = b.Name,
                Code = b.Code,
                BeginingBalance = b.BeginingBalance,
                EndingBalance = b.EndingBalance
            }).ToArray()
        };
    }

    #endregion
}