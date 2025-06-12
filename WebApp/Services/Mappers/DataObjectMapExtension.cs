using System.Dynamic;
using System.Reflection;
using System.Text.Json;
using Microsoft.IdentityModel.Tokens;
using WebApp.Core.DomainEntities;
using WebApp.Core.DomainEntities.Accounting;
using WebApp.Core.DomainEntities.Salary;
using WebApp.Repositories;
using WebApp.Services.BalanceSheetService.Dto;
using WebApp.Services.CommonService;
using WebApp.Services.OrganizationService.Dto;
using WebApp.Services.PayrollService.Dto;
using WebApp.Services.RegionService.Dto;
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
                    AlterName = o.District.AlterName,
                    ProvinceId = o.District.Province?.Id,
                    ProvinceName = o.District.Province?.Name,
                },
            Emails = o.Emails,
            Phones = o.Phones,
            ContactAddress = o.ContactAddress,
            TaxId = o.TaxId,
            TaxOffice = o.TaxOffice is null
                ? null
                : new TaxOfficeDisplayDto
                {
                    Id = o.TaxOffice.Id,
                    FullName = o.TaxOffice.FullName,
                    ShortName = o.TaxOffice.ShortName,
                    Code = o.TaxOffice.Code,
                    ParentId = o.TaxOffice.ParentId,
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
            Emails = i.Emails.Select(x => x.RemoveSpace()!).ToList(),
            Phones = i.Phones.Select(x => x.RemoveSpace()!).ToList(),
            InvoicePwd = string.IsNullOrEmpty(i.InvoicePwd) ? null : i.InvoicePwd,
            PinCode = string.IsNullOrEmpty(i.PinCode) ? null : i.PinCode,
            TaxId = i.TaxId.RemoveSpace()!,
            UnsignName = i.FullName.RemoveSpace()!.UnSign(),
            TaxIdPwd = string.IsNullOrEmpty(i.TaxIdPwd) ? null : i.TaxIdPwd,
            TypeOfVatPeriod = i.TypeOfVatPeriod.RemoveSpace() ?? "Q",
            OrganizationLoginInfos = i.OrganizationLoginInfos
                                      .Select(x => new OrganizationLoginInfo
                                      {
                                          AccountName = x.AccountName,
                                          Password = x.Password,
                                          Provider = x.Provider,
                                          Url = x.Url,
                                          Username = x.Username
                                      })
                                      .ToHashSet()
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
                    Id = o.Parent.Id, FullName = o.Parent.FullName
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
                Id = o.Id, FullName = o.FullName, TaxId = o.TaxId
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

    #region ImportedBalanceSheet

    public static ImportedBalanceSheetDetail ToEntity(this ImportedBsDetailCreateDto i)
    {
        return new ImportedBalanceSheetDetail
        {
            Account = i.Account,
            OpenCredit = i.OpenCredit,
            OpenDebit = i.OpenDebit,
            AriseCredit = i.AriseCredit,
            AriseDebit = i.AriseDebit,
            CloseCredit = i.CloseCredit,
            CloseDebit = i.CloseDebit,
            IsValid = i.IsValid,
            Note = i.Note,
        };
    }

    public static ImportedBsDetailDisplayDto ToDisplayDto(this ImportedBalanceSheetDetail d)
    {
        return new ImportedBsDetailDisplayDto
        {
            Account = d.Account,
            Id = d.Id,
            Note = d.Note,
            AriseCredit = d.AriseCredit,
            AriseDebit = d.AriseDebit,
            CloseCredit = d.CloseCredit,
            CloseDebit = d.CloseDebit,
            OpenCredit = d.OpenCredit,
            OpenDebit = d.OpenDebit,
            IsValid = d.IsValid,
        };
    }

    public static ImportedBsDisplayDto ToDisplayDto(this ImportedBalanceSheet i)
    {
        return new ImportedBsDisplayDto
        {
            Id = i.Id,
            Name = i.Name,
            BalanceSheetId = i.BalanceSheetId,
            Year = i.Year,
            OrganizationId = i.Organization?.Id.ToString(),
            Details = i.Details.IsNullOrEmpty()
                ? []
                : i.Details.MapCollection(x => x.ToDisplayDto())
                   .OrderBy(x => x.Account)
                   .ToList(),
        };
    }

    #endregion

    #region ACCOUNT TEMPLATE

    public static Account ToEntity(this AccountCreateDto d)
    {
        return new Account()
        {
            Id = int.Parse(d.AccountNumber),
            AccountNumber = d.AccountNumber,
            Name = d.Name.RemoveSpace() ?? string.Empty,
            Deleted = false,
            Parent = d.Parent,
            Grade = d.AccountNumber.Length - 2,
            B02 = d.B02,
            B01TS = d.B01TS,
            B01NV = d.B01NV
        };
    }

    #endregion

    #region PAYROLL

    #region Employee

    public static EmployeeDisplayDto ToDisplayDto(this Employee e)
    {
        return new EmployeeDisplayDto
        {
            Id = e.Id,
            FullName = e.FullName,
            Pid = e.Pid,
            TaxId = e.TaxId,
            HireDate = e.HireDate,
            TerminationDate = e.TerminationDate,
            IsActive = e.IsActive,
            OrganizationId = e.OrganizationId,
            Salary = e.BaseSalary,
            InsuranceSalary = e.InsuranceSalary,
            Dependents = e.Dependents.Select(d => d.ToDisplayDto()).ToList(),
        };
    }

    public static Employee ToEntity(this EmployeeCreateDto dto, Organization org)
    {
        return new Employee
        {
            FullName = dto.FullName,
            Pid = dto.Pid,
            TaxId = dto.TaxId,
            HireDate = dto.HireDate,
            TerminationDate = dto.TerminationDate,
            Organization = org
        };
    }

    public static void UpdateEntity(this EmployeeCreateDto dto, Employee e)
    {
        e.FullName = dto.FullName;
        e.Pid = dto.Pid;
        e.TaxId = dto.TaxId;
        e.HireDate = dto.HireDate;
        e.TerminationDate = dto.TerminationDate;
    }

    // Maps DependentCreateDto to Dependent entity
    public static Dependent ToEntity(this DependentCreateDto dto, Employee employee)
    {
        return new Dependent
        {
            FullName = dto.FullName,
            TaxId = dto.TaxId,
            Pid = dto.Pid,
            Relationship = dto.Relationship,
            DateOfBirth = dto.DateOfBirth,
            EffectiveDate = dto.EffectiveDate,
            EndDate = dto.EndDate,
            IsActive = dto.IsActive,
            Employee = employee,
            EmployeeId = employee.Id
        };
    }

    // Maps Dependent entity to DependentDisplayDto
    public static DependentDisplayDto ToDisplayDto(this Dependent entity)
    {
        return new DependentDisplayDto
        {
            Id = entity.Id,
            FullName = entity.FullName,
            TaxId = entity.TaxId,
            Pid = entity.Pid,
            Relationship = entity.Relationship,
            DateOfBirth = entity.DateOfBirth,
            EffectiveDate = entity.EffectiveDate,
            EndDate = entity.EndDate,
            IsActive = entity.IsActive,
            EmployeeId = entity.EmployeeId
        };
    }

    #endregion

    #region PayrollRecord

    public static PayrollRecord ToEntity(this PayrollRecordCreateDto dto,
                                         Employee e,
                                         PayrollPeriod period)
    {
        return new PayrollRecord
        {
            Employee = e,
            PayrollPeriod = period,
            TotalDeduction = dto.TotalDeduction,
            TotalGrossPay = dto.TotalGrossPay,
            TotalNetPay = dto.TotalNetPay,
            IsClosed = dto.IsClosed,
            TaxType = dto.TaxType,
        };
    }

    public static PayrollRecordDisplayDto ToDisplayDto(this PayrollRecord entity)
    {
        return new PayrollRecordDisplayDto
        {
            Id = entity.Id,
            EmployeeId = entity.EmployeeId,
            PeriodId = entity.PayrollPeriodId,
            EmployeeName = entity.Employee.FullName,
            PeriodName = entity.PayrollPeriod.Name,
            IsClosed = entity.IsClosed,
            TotalDeduction = entity.TotalDeduction,
            TotalGrossPay = entity.TotalGrossPay,
            TotalNetPay = entity.TotalNetPay,
        };
    }

    #endregion

    #region PayrollPeriod

    // Maps PayrollPeriodCreateDto to PayrollPeriod entity
    public static PayrollPeriod ToEntity(this PayrollPeriodCreateDto dto, Organization org)
    {
        return new PayrollPeriod
        {
            Name = dto.Name,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            NetWorkDays = dto.NetWorkDays,
            Organization = org,
            OrganizationId = org.Id,
            Year = dto.Year,
            Month = dto.Month,
            WeekendType = dto.WeekendType,
            IsFinal = dto.IsFinal,
            Version = dto.Version,
        };
    }

    // Maps PayrollPeriod entity to PayrollPeriodDisplayDto
    public static PayrollPeriodDisplayDto ToDisplayDto(this PayrollPeriod entity)
    {
        return new PayrollPeriodDisplayDto
        {
            Id = entity.Id,
            Name = entity.Name,
            StartDate = entity.StartDate,
            EndDate = entity.EndDate,
            NetWorkDays = entity.NetWorkDays,
            IsClosed = entity.IsClosed,
            Year = entity.Year,
            Month = entity.Month,
            WeekendType = nameof(entity.WeekendType),
            IsFinal = entity.IsFinal,
            Version = entity.Version,
        };
    }

    // Updates an existing PayrollPeriod entity from PayrollPeriodCreateDto
    public static void UpdateEntity(this PayrollPeriodCreateDto dto, PayrollPeriod entity)
    {
        entity.Name = dto.Name;
        entity.StartDate = dto.StartDate;
        entity.EndDate = dto.EndDate;
        entity.NetWorkDays = dto.NetWorkDays;
        entity.Year = dto.Year;
        entity.Month = dto.Month;
        // Organization and OrganizationId are not updated here
    }

    #endregion

    #region PayrollComponentCategory

    public static PayrollComponentCategory ToEntity(this PayrollComponentCategoryCreateDto dto)
    {
        return new PayrollComponentCategory
        {
            Name = dto.Name,
            Description = dto.Description,
            Order = dto.Order
        };
    }

    public static PayrollComponentCategoryDisplayDto ToDisplayDto(this PayrollComponentCategory entity)
    {
        return new PayrollComponentCategoryDisplayDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Description = entity.Description,
            Order = entity.Order
        };
    }

    #endregion

    #region Payroll InputType

    public static PayrollInputType ToEntity(this PayrollInputTypeCreateDto dto)
    {
        return new PayrollInputType
        {
            Name = dto.Name,
            Description = dto.Description,
            Unit = dto.Unit,
            DataType = dto.DataType,
        };
    }

    #endregion

    #region Payroll Input

    public static PayrollInput ToEntity(this PayrollInputCreateDto dto,
                                        PayrollInputType payrollInputType,
                                        PayrollRecord payrollRecord)
    {
        return new PayrollInput
        {
            Value = dto.Value,
            PayrollInputType = payrollInputType,
            PayrollRecord = payrollRecord,
        };
    }

    public static PayrollInputDisplayDto ToDisplayDto(this PayrollInput entity)
    {
        return new PayrollInputDisplayDto
        {
            Id = entity.Id,
            Value = entity.Value,
            PayrollInputTypeId = entity.PayrollInputTypeId,
            PayrollRecordId = entity.PayrollRecordId,
            TypeName = entity.PayrollInputType.Name,
        };
    }

    #endregion

    #region PayrollComponentType

    public static PayrollComponentType ToEntity(this PayrollComponentTypeCreateDto dto)
    {
        return new PayrollComponentType
        {
            Name = dto.Name,
            Description = dto.Description,
            DataSourceType = dto.DataSourceType,
            OrganizationId = dto.OrganizationId,
            PayrollComponentCategoryId = dto.PayrollComponentCategoryId,
            IsActive = dto.IsActive,
            IsDeductible = dto.IsDeductible,
            IsTaxable = dto.IsTaxable,
            InputTypeId = dto.InputTypeId,
            DataType = dto.DataType
        };
    }

    public static PayrollComponentTypeDisplayDto ToDisplayDto(this PayrollComponentType entity)
    {
        return new PayrollComponentTypeDisplayDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Description = entity.Description,
            DataSourceType = entity.DataSourceType,
            OrganizationId = entity.OrganizationId,
            PayrollComponentCategoryId = entity.PayrollComponentCategoryId,
            IsActive = entity.IsActive,
            IsDeductible = entity.IsDeductible,
            IsTaxable = entity.IsTaxable,
            InputTypeId = entity.InputTypeId,
            DataType = entity.DataType
        };
    }

    #endregion

    #region Timesheet

    public static void UpdateEntity(this TimesheetUpdateDto dto, Timesheet entity)
    {
        entity.Description = dto.Description;
        entity.IsHoliday = dto.IsHoliday;
        entity.IsTripDay = dto.IsTripDay;
        entity.Leave = dto.Leave;
        // EmployeeId and PayrollRecordId are not updated here
    }

    #endregion

    #endregion
}