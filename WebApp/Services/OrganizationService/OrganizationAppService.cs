using System.Diagnostics.CodeAnalysis;
using System.Linq.Dynamic.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver.Core.WireProtocol.Messages;
using WebApp.Core.DomainEntities;
using WebApp.Mongo.DocumentModel;
using WebApp.Mongo.MongoRepositories;
using WebApp.Payloads;
using WebApp.Repositories;
using WebApp.Services.CommonService;
using WebApp.Services.Mappers;
using WebApp.Services.OrganizationService.Dto;
using WebApp.Services.UserService;
using WebApp.Utils;
using X.Extensions.PagedList.EF;
using X.PagedList;
using X.PagedList.Extensions;
using ResponseMessage = WebApp.Enums.ResponseMessage;

namespace WebApp.Services.OrganizationService;

public interface IOrganizationAppService
{
    Task<ResponseEntity> Create(OrganizationInputDto dto);
    Task<ResponseEntity> CreateMany(List<OrganizationInputDto> dto);
    Task<ResponseEntity> GetAllOrgByCurrentUserAsync(PageRequest req);
    Task<ResponseEntity> GetOneById(Guid id);
    Task<ResponseEntity> CheckTaxIdExist(string taxId);
    Task<ResponseEntity> Update(Guid orgId, OrganizationInputDto updateDto);
    Task<ResponseEntity> GetAllOrgForAdmin(PageRequest req);
}

public class OrganizationBaseAppService(IAppRepository<Organization, Guid> orgRepo,
                                        IAppRepository<Province, int> provinceRepo,
                                        IAppRepository<District, int> districtRepo,
                                        IAppRepository<TaxOffice, int> taxOfficeRepo,
                                        IAppRepository<TaxOffice2, int> taxOffice2Repo,
                                        IAppRepository<User, Guid> userRepo,
                                        IAppRepository<OrganizationLoginInfo, int> loginInfoRepo,
                                        IOrgMongoRepository orgMongoRepository,
                                        IUserManager userManager) : BaseAppService(userManager), IOrganizationAppService
{
    public async Task<ResponseEntity> Create(OrganizationInputDto dto)
    {
        if (await TaxIdExist(dto.TaxId))
            return ResponseEntity.Error("Tax Id has already existed.");

        var invalidMessage = await ValidInputDto(dto);
        if (invalidMessage.Count > 0) return ResponseEntity.Error("Invalid input", invalidMessage);

        var newOrg = dto.ToEntity();

        // Attach location:
        newOrg.District = districtRepo.Attach(dto.DistrictId!.Value);
        //newOrg.TaxOffice = taxOfficeRepo.Attach(dto.TaxOfficeId!.Value);
        newOrg.TaxOffice2 = taxOffice2Repo.Attach(dto.TaxOfficeId!.Value);
        // Add the user who create the new organization to its users list:
        if (UserId is not null && Guid.TryParse(UserId, out var uId))
        {
            newOrg.Users.Add(userRepo.Attach(uId));
        }

        var saved = await orgRepo.CreateAsync(newOrg);
        //store new Id in mongo:
        //await orgMongoRepository.InsertOrgId(new OrgDoc { OrgId = saved.Id.ToString() });
        return ResponseEntity.OkResult(saved.ToDisplayDto());
    }

    public async Task<ResponseEntity> CreateMany(List<OrganizationInputDto> input)
    {
        var duplicateTaxIds = input.GroupBy(o => o.TaxId)
                                   .Where(c => c.Count() > 1)
                                   .SelectMany(o => o).ToList();

        var distinctTaxIds = input.GroupBy(o => o.TaxId)
                                  .Where(o => o.Count() == 1)
                                  .SelectMany(o => o).ToList();

        var currentTaxIds = orgRepo.GetQueryable().Select(d => d.TaxId).ToHashSet();

        var existingTaxIds = distinctTaxIds.Where(x => currentTaxIds.Contains(x.TaxId)).ToList();

        var validTaxIds = distinctTaxIds.Except(existingTaxIds).ToList(); //the dto list contains only passed taxId

        var taxOfficeIds = taxOfficeRepo.GetQueryable().Select(t => t.Id).ToHashSet();
        var districtIds = districtRepo.GetQueryable().Select(d => d.Id).ToHashSet();

        var invalidTaxOfficeIds = input.Where(x => x.TaxOfficeId is null || !taxOfficeIds.Contains(x.TaxOfficeId.Value))
                                       .ToList();
        var invalidDistrictIds = input.Where(x => x.DistrictId is null || !districtIds.Contains(x.DistrictId.Value))
                                      .ToList();

        var validDtos = validTaxIds.Except(invalidTaxOfficeIds).Except(invalidDistrictIds).ToList();

        if (validDtos.Count == 0)
            return new ResponseEntity
            {
                Message = "All inputs are invalid",
                Success = false,
                Data = new
                {
                    totalItems = input.Count,
                    insertedItems = 0,
                    invalidItems = new
                    {
                        duplicateTaxIds,
                        existingTaxIds,
                        invalidTaxOfficeIds,
                        invalidDistrictIds
                    }
                }
            };

        var entitiesToSave = validDtos.Select(dto =>
        {
            var org = dto.ToEntity();
            org.TaxOffice = taxOfficeRepo.Attach(dto.TaxOfficeId!.Value);
            //org.District = districtRepo.Attach(dto.DistrictId!.Value);
            return org;
        }).ToList();

        await orgRepo.CreateManyAsync(entitiesToSave);


        return new ResponseEntity
        {
            Success = true,
            Message =
                $"Successfully added {entitiesToSave.Count}/{input.Count} organization(s). Check data for error (if any)",
            Data = new
            {
                totalItems = input.Count,
                insertedItems = entitiesToSave.Count,
                invalidItems = new
                {
                    duplicateTaxIds,
                    existingTaxIds,
                    invalidTaxOfficeIds,
                    invalidDistrictIds
                }
            }
        };
    }

    public async Task<ResponseEntity> CheckTaxIdExist(string taxId)
    {
        return await TaxIdExist(taxId)
            ? new ResponseEntity { Success = false, Message = "TaxId has already existed" }
            : new ResponseEntity { Success = true, Message = "OK" };
    }

    public async Task<ResponseEntity> GetAllOrgForAdmin(PageRequest req)
    {
        var keyword = req.Keyword.RemoveSpace()?.UnSign();
        var result = (await orgRepo.Find(filter: o => !o.Deleted &&
                                                      (string.IsNullOrEmpty(keyword) ||
                                                       o.UnsignName.Contains(keyword) ||
                                                       (o.ShortName != null &&
                                                        o.ShortName.Contains(keyword)) ||
                                                       o.TaxId.Contains(keyword)),
                                         sortBy: req.SortBy, order: req.OrderBy,
                                         include:
                                         [
                                             nameof(Organization.TaxOffice),
                                             nameof(Organization.District),
                                             nameof(Organization.Users)
                                         ])
                                   .AsSplitQuery()
                                   .AsNoTracking()
                                   .ToPagedListAsync(req.Page, req.Size)).MapPagedList(x => x.ToDisplayDto());
        ;
        return req.Fields.Length == 0
            ? ResponseEntity.OkResult(result) //If no fields are specified, return all fields
            : ResponseEntity.OkResult(result.ProjectPagedList(req.Fields)); //return only specified fields
    }

    public async Task<ResponseEntity> GetAllOrgByCurrentUserAsync(PageRequest req)
    {
        var userId = UserId;
        var keyword = req.Keyword.RemoveSpace()?.UnSign();
        var query = await orgRepo.Find(filter: o => !o.Deleted &&
                                                    o.Users.Any(u => u.Id.ToString() == userId) &&
                                                    (string.IsNullOrEmpty(keyword) ||
                                                     o.UnsignName.Contains(keyword) ||
                                                     (o.ShortName != null &&
                                                      o.ShortName.Contains(keyword)) ||
                                                     o.TaxId.Contains(keyword)),
                                       sortBy: req.SortBy, order: req.OrderBy,
                                       include:
                                       [
                                           nameof(Organization.TaxOffice2),
                                           nameof(Organization.District)
                                       ])
                                 .Include(o => o.Users.Where(u => !u.Deleted))
                                 .AsSplitQuery()
                                 .AsNoTracking()
                                 .ToPagedListAsync(req.Page, req.Size);

        return req.Fields.Length == 0
            ? ResponseEntity.OkResult(query.MapPagedList(x => x.ToDisplayDto()))
            : ResponseEntity.OkResult(query.ProjectPagedList(req.Fields));
    }

    public async Task<ResponseEntity> Update(Guid orgId, OrganizationInputDto updateDto)
    {
        var invalidMessage = await ValidInputDto(updateDto);
        if (invalidMessage.Count > 0) return ResponseEntity.Error("Invalid input", invalidMessage);

        var foundOrg = await orgRepo.Find(o => o.Id == orgId && !o.Deleted,
                                          include: nameof(Organization.OrganizationLoginInfos))
                                    .FirstOrDefaultAsync();

        if (foundOrg is null)
        {
            return new ResponseEntity { Success = false, Message = "Organization Id not found" };
        }

        if (await TaxIdExist(updateDto.TaxId) && updateDto.TaxId != foundOrg.TaxId)
        {
            return new ResponseEntity { Success = false, Message = "The TaxId you enter has already existed" };
        }

        updateDto.UpdateEntity(foundOrg);

        Console.WriteLine($"OWNER TYPE = {updateDto.CapitalOwnershipType}");

        foundOrg.District = districtRepo.Attach(updateDto.DistrictId!.Value);
        foundOrg.TaxOffice2 = taxOffice2Repo.Attach(updateDto.TaxOfficeId!.Value);

        //update login info:
        List<OrganizationLoginInfo> updateList = [];
        //get all existing login infos by organization id:
        Dictionary<int, OrganizationLoginInfo> existList = foundOrg.OrganizationLoginInfos.ToDictionary(x => x.Id);
        foreach (var loginInfo in updateDto.OrganizationLoginInfos)
        {
            //check if it's a new item
            if (loginInfo.Id == null)
            {
                var newLoginInfo = new OrganizationLoginInfo
                {
                    Username = loginInfo.Username,
                    Provider = loginInfo.Provider,
                    Password = loginInfo.Password,
                    Url = loginInfo.Url,
                    AccountName = loginInfo.AccountName,
                    OrganizationId = foundOrg.Id
                };
                updateList.Add(newLoginInfo);
                continue;
            }

            //check if this item exists
            if (!existList.TryGetValue(loginInfo.Id.Value, out var found))
            {
                continue; //skip this item because it doesn't exist
            }

            //update existing item
            found.AccountName = loginInfo.AccountName;
            found.Provider = loginInfo.Provider;
            found.Url = loginInfo.Url;
            found.Username = loginInfo.Username;
            found.Password = loginInfo.Password;
            updateList.Add(found);
            existList.Remove(loginInfo.Id.Value); //remove from exist list
        }

        //The rest of items in existList will be deleted:
        foreach (var toDelete in existList.Values)
        {
            foundOrg.OrganizationLoginInfos.Remove(toDelete);
        }

        //add new items into existList:
        foundOrg.OrganizationLoginInfos = updateList.ToHashSet();
        var saved = await orgRepo.UpdateAsync(foundOrg);
        return new ResponseEntity { Success = true, Data = saved.Id, Message = "Update successfully" };
    }

    public async Task<ResponseEntity> GetOneById(Guid id)
    {
        var org = await orgRepo.Find(filter: x => x.Id == id,
                                     include: [nameof(Organization.OrganizationLoginInfos)])
                               .Include(x => x.District)
                               .Include(x => x.TaxOffice2)
                               .FirstOrDefaultAsync();
        return org == null
            ? ResponseEntity.Error(ResponseMessage.NotFound)
            : ResponseEntity.OkResult(org.ToDisplayDto());
    }

    private async Task<bool> TaxIdExist(string taxId)
    {
        return await orgRepo.ExistAsync(o => o.TaxId == taxId);
    }

    private async Task<List<string>> ValidInputDto(OrganizationInputDto dto)
    {
        var errors = new List<string>();

        if (dto.TaxOfficeId is null || !await taxOffice2Repo.ExistAsync(x => x.Id == dto.TaxOfficeId))
        {
            errors.Add("Invalid tax office or tax office not found");
        }

        /*if (dto.DistrictId is null || !await districtRepo.ExistAsync(x => x.Id == dto.DistrictId))
        {
            errors.Add("Invalid district or district not found");
        }*/

        return errors;
    }
}