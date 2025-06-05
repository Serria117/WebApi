using System.Diagnostics;
using System.Linq.Dynamic.Core;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using WebApp.Core.DomainEntities;
using WebApp.Enums;
using WebApp.Mongo.DocumentModel;
using WebApp.Mongo.MongoRepositories;
using WebApp.Payloads;
using WebApp.Repositories;
using WebApp.Services.CachingServices;
using WebApp.Services.Mappers;
using WebApp.Services.UserService.Dto;
using X.Extensions.PagedList.EF;
using X.PagedList;

namespace WebApp.Services.UserService
{
    public interface IRoleAppService
    {
        Task<RoleDisplayDto> CreateRole(RoleInputDto dto);

        Task<AppResponse> GetRoleById(int id);

        Task<AppResponse> GetAllRoles(PageRequest request);

        /// <summary>
        /// Updates an existing role with the specified details.
        /// <param name="roleId">The ID of the role to update.</param>
        /// <param name="dto">The new details of the role.</param>
        /// </summary>
        Task UpdateRole(int roleId, RoleInputDto dto);

        /// <summary>
        /// Retrieves all permissions associated with a specific role.
        /// </summary>
        /// <param name="roleId">The ID of the role whose permissions are to be retrieved.</param>
        /// <returns>An <see cref="AppResponse"/> containing the permissions of the specified role.</returns>
        Task<AppResponse> GetAllPermissionsInRole(int roleId);

        Task DeleteRole(int roleId);
        Task<AppResponse> FindRoleById(int id);
    }

    public class RoleAppService(IAppRepository<User, Guid> userRepository,
                                IAppRepository<Role, int> roleRepository,
                                IAppRepository<Permission, int> permissionRepository,
                                IUserMongoRepository userMongoRepository,
                                ICachingRoleService cachingRoleService,
                                ILogger<RoleAppService> logger) : IRoleAppService
    {
        public async Task<AppResponse> GetRoleById(int id)
        {
            var found = await roleRepository.Find(x => x.Id == id && !x.Deleted)
                                            .Include(x => x.Permissions)
                                            .Include(x => x.Users)
                                            .FirstOrDefaultAsync();
            return found is null
                ? AppResponse.Error404("Role not found")
                : AppResponse.OkResult(found.ToDisplayDto());
        }

        public async Task<AppResponse> GetAllRoles(PageRequest request)
        {
            var pagedResult = await roleRepository
                                    .Find(
                                        filter: r =>
                                            !r.Deleted && (string.IsNullOrEmpty(request.Keyword) ||
                                                           r.RoleName.Contains(request.Keyword)),
                                        sortBy: request.SortBy,
                                        order: request.OrderBy,
                                        include: [nameof(Role.Permissions)])
                                    .AsSplitQuery()
                                    .AsNoTracking()
                                    .ToPagedListAsync(request.Page, request.Size);
            var dtoResult = pagedResult.MapPagedList(x => x.ToDisplayDto());
            return request.Fields.Length == 0
                ? AppResponse.OkResult(dtoResult)
                : AppResponse.OkResult(dtoResult.ProjectPagedList(request.Fields));
        }

        public async Task<AppResponse> GetAllPermissionsInRole(int roleId)
        {
            var role = await roleRepository.Find(r => r.Id == roleId)
                                           .Include(r => r.Permissions).FirstOrDefaultAsync();
            return role is null
                ? new AppResponse { Success = false, Message = "Role not found" }
                : AppResponse.OkResult(role.ToDisplayDto());
        }

        //TODO: implementation for add and remove user from role
        public async Task DeleteRole(int roleId)
        {
            if (await roleRepository.SoftDeleteAsync(roleId))
            {
                //TODO: remove users from role
            }
        }

        public async Task<RoleDisplayDto> CreateRole(RoleInputDto dto)
        {
            var role = dto.ToEntity();
            var permissions = await FindAllPermissionById([.. dto.Permissions]);

            if (permissions.Count > 0)
                role.Permissions.UnionWith(permissions);

            await AddUsersToRole(role, dto.Users);

            var saved = await roleRepository.CreateAsync(role);
            await cachingRoleService.GetPermissionsFromCache(saved.RoleName); // Add to cache
            return saved.ToDisplayDto();
        }

        public async Task<AppResponse> FindRoleById(int id)
        {
            var role = await roleRepository.Find(filter: r => r.Id == id && !r.Deleted, 
                                                 include: [nameof(Role.Users), nameof(Role.Permissions)])
                                           .FirstOrDefaultAsync();
            return role is null
                ? new AppResponse { Success = false, Message = "Role not found" }
                : AppResponse.OkResult(role.ToDisplayDto());
        }

        //TODO: re-test this method for potential bugs
        public async Task UpdateRole(int roleId, RoleInputDto dto)
        {
            var role = await roleRepository.Find(r => r.Id == roleId)
                                           .Include(r => r.Permissions)
                                           .Include(r => r.Users)
                                           .FirstOrDefaultAsync();
            if (role is null) throw new Exception("Role id not found");
            cachingRoleService.InvalidateCacheForRole(role.RoleName); // Invalidate cache for the role
            dto.UpdateEntity(role);
            var newPermissions = await FindAllPermissionById([.. dto.Permissions]);

            var permissionsToRemove = role.Permissions.Except(newPermissions).ToList();
            var permissionsToAdd = newPermissions.Except(role.Permissions).ToList();

            foreach (var permission in permissionsToRemove)
            {
                role.Permissions.Remove(permission);
            }

            foreach (var permission in permissionsToAdd)
            {
                role.Permissions.Add(permission);
            }
            
            //TODO: update users in role
            await AddUsersToRole(role, dto.Users);
            
            await roleRepository.UpdateAsync(role);
            await cachingRoleService.GetPermissionsFromCache(role.RoleName); // Cache the updated role permissions
        }

        private async Task<List<User>> GetUsersHaveRole(int roleId)
        {
            List<User> users = [];
            var role = await roleRepository.Find(r => r.Id == roleId && !r.Deleted).FirstOrDefaultAsync();
            if (role is not null)
                users.AddRange(await userRepository.Find(u => u.Roles.Contains(role) && !u.Deleted)
                                                   .Include(u => u.Roles).ThenInclude(r => r.Permissions)
                                                   .AsSplitQuery()
                                                   .ToListAsync());
            return users;
        }


        private async Task UpdatePermissionForAllUsers(int roleId)
        {
            var foundUsers = await GetUsersHaveRole(roleId);
            if (foundUsers.Count == 0) return;
            var userDocs = foundUsers.Select(u => new UserDoc
                                     {
                                         UserId = u.Id.ToString(),
                                         Permissions = u.Roles.SelectMany(r => r.Permissions)
                                                        .Select(p => p.PermissionName).ToHashSet()
                                     })
                                     .ToList();
            await userMongoRepository.UpdateAllUser(userDocs);
        }


        private async Task<List<Permission>> FindAllPermissionById(List<int> ids)
        {
            return await permissionRepository.Find(p => ids.Contains(p.Id)).ToListAsync();
        }

        private async Task AddUsersToRole(Role role, ICollection<Guid> userIds)
        {
            var users = await userRepository
                              .Find(u => userIds.Contains(u.Id) && !u.Deleted)
                              .ToListAsync();
        
            // Remove users not in userIds
            var usersToRemove = role.Users.Where(u => !userIds.Contains(u.Id)).ToList();
            foreach (var user in usersToRemove)
            {
                role.Users.Remove(user);
            }
        
            // Add new users
            role.Users.UnionWith(users);
        }
        
    }
}