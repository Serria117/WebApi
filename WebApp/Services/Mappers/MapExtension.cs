using Microsoft.IdentityModel.Tokens;
using WebApp.Core.DomainEntities;
using WebApp.Core.DomainEntities.Accounting;
using WebApp.Repositories;
using WebApp.Services.BalanceSheetService.Dto;
using WebApp.Services.CommonService;
using WebApp.Services.OrganizationService.Dto;
using WebApp.Services.RegionService.Dto;
using WebApp.Services.UserService.Dto;
using X.PagedList;

namespace WebApp.Services.Mappers;

public static class MapExtension
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
            FiscalYearFirstDate = o.FiscalYearFistDate
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
            InvoicePwd = i.InvoicePwd,
            PinCode = i.PinCode,
            TaxId = i.TaxId.RemoveSpace()!,
            UnsignName = i.FullName.RemoveSpace()!.UnSign(),
            TaxIdPwd = i.TaxIdPwd,
        };
    }

    public static void UpdateEntity(this OrganizationInputDto i, Organization o)
    {
        o.FullName = i.FullName.RemoveSpace() ?? o.FullName;
        o.ContactAddress = i.ContactAddress.RemoveSpace() ?? o.ContactAddress;
        o.ShortName = i.ShortName.RemoveSpace() ?? o.ShortName;
        o.Emails = i.Emails.IsNullOrEmpty() ? [] : i.Emails.Select(x => x.RemoveSpace()!).ToList();
        o.Address = i.Address.RemoveSpace() ?? o.Address;
        o.UnsignName = o.FullName.UnSign();
        o.Phones = i.Phones.IsNullOrEmpty() ? [] : i.Phones.Select(x => x.RemoveSpace()!).ToList();
        o.InvoicePwd = i.InvoicePwd.RemoveSpace() ?? o.InvoicePwd;
        o.TaxIdPwd = i.TaxIdPwd.RemoveSpace() ?? o.TaxIdPwd;
        o.PinCode = i.PinCode.RemoveSpace() ?? o.PinCode;
        o.TaxId = i.TaxId.RemoveSpace() ?? o.TaxId;
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
            Province = o.Province?.Name,
        };
    }

    public static TaxOffice ToEntity(this TaxOfficeCreateDto d, IAppRepository<Province, int> provinceRepo)
    {
        return new TaxOffice
        {
            FullName = d.FullName.RemoveSpace()!,
            ShortName = d.ShortName.RemoveSpace()!,
            Code = d.Code.RemoveSpace()!,
            ParentId = d.ParentId,
            Province = provinceRepo.Attach(id: d.ProvinceId)
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
            Roles = u.Roles.Select(x => new RoleDisplayDto
            {
                Id = x.Id,
                RoleName = x.RoleName
            }).ToHashSet(),
            Organizations = u.Organizations.Select(o => new OrganizationInUserDto()
            {
                Id = o.Id, FullName = o.FullName, TaxId = o.TaxId
            }).ToList()
        };
    }

    public static User ToEntity(this UserInputDto d)
    {
        return new User
        {
            Username = d.Username.RemoveSpace()!,
            Password = d.Password.BCryptHash(),
        };
    }

    public static void UpdateEntity(this User u, User e)
    {
        e.Password = u.Password.BCryptHash();
    }

    public static RoleDisplayDto ToDisplayDto(this Role role)
    {
        return new RoleDisplayDto
        {
            Id = role.Id,
            RoleName = role.RoleName,
            Description = role.Description,
            Permissions = role.Permissions.MapCollection(x => x.ToDisplayDto()).ToHashSet(),
            Users = role.Users.Count == 0 ? [] : role.Users.Select(u => new UserInfoDto()
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
        return new PermissionDisplayDto()
        {
            Id = permission.Id,
            Description = permission.Description,
            PermissionName = permission.PermissionName,
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
}