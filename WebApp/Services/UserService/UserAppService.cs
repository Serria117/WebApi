using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Linq.Dynamic.Core;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using WebApp.Core.DomainEntities;
using WebApp.Mongo.DocumentModel;
using WebApp.Mongo.MongoRepositories;
using WebApp.Payloads;
using WebApp.Repositories;
using WebApp.Services.CommonService;
using WebApp.Services.Mappers;
using WebApp.Services.UserService.Dto;
using X.Extensions.PagedList.EF;

namespace WebApp.Services.UserService
{
    public interface IUserAppService
    {
        Task<UserDisplayDto> CreateUser(UserInputDto user);

        /// <summary>
        /// Authenticates the user with the provided login details.
        /// </summary>
        /// <param name="login">The login details of the user.</param>
        /// <returns>An <see cref="AuthenticationResponse"/> containing the authentication result.</returns>
        Task<AuthenticationResponse> Authenticate(UserLoginDto login);

        Task<bool> ExistUsername(string username);
        Task<(User? User, ISet<string> Permisions)> FindUserByUserName(string username);
        Task<List<Role>> FindAllRoles(ICollection<int> roleIds);

        /// <summary>
        /// Finds roles associated with a specific user.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <returns>A list of roles associated with the user.</returns>
        Task<List<RoleDisplayDto>> FindRolesByUser(Guid userId);

        /// <summary>
        /// Retrieves all users with pagination.
        /// </summary>
        /// <param name="page">The page request containing pagination details.</param>
        /// <returns>A paginated list of users.</returns>
        Task<AppResponse> GetAllUsers(PageRequest page);

        /// <summary>
        /// Unlocks a user account.
        /// </summary>
        /// <param name="userId">The ID of the user to unlock.</param>
        Task UnlockUser(Guid userId);

        /// <summary>
        /// Changes the roles of a user.
        /// </summary>
        /// <param name="id">The ID of the user.</param>
        /// <param name="roleIds">The list of role IDs to assign to the user.</param>
        /// <returns>A response indicating the result of the operation.</returns>
        Task<AppResponse> ChangeUserRoles(Guid id, List<int> roleIds);

        /// <summary>
        /// Allows a user to change their own password.
        /// </summary>
        /// <param name="oldPassword">The current password of the user.</param>
        /// <param name="newPassword">The new password to set.</param>
        /// <returns>A response indicating the result of the operation.</returns>
        Task<AppResponse> SelfChangePassword(string oldPassword, string newPassword);

        /// <summary>
        /// Adds organizations to a user.
        /// </summary>
        /// <param name="user">The ID of the user.</param>
        /// <param name="orgIds">The collection of organization IDs to add to the user.</param>
        /// <returns>A response indicating the result of the operation.</returns>
        Task<AppResponse> AddOrganizationToUser(Guid user, ICollection<Guid> orgIds);

        /// <summary>
        /// Changes the working organization for the user.
        /// </summary>
        /// <param name="orgId">The ID of the organization to switch to.</param>
        /// <param name="refreshToken"></param>
        /// <returns>An authentication response containing the new access token and refresh token.</returns>
        public Task<AuthenticationResponse> ChangeWorkingOrganization(string orgId, string refreshToken);

        /// <summary>
        /// Refreshes the access token using the provided refresh token.
        /// </summary>
        /// <param name="currentRefreshToken">The current refresh token.</param>
        /// <returns>An <see cref="AuthenticationResponse"/> containing the new access token and refresh token.</returns>
        Task<AuthenticationResponse> RefreshTokenAsync(string currentRefreshToken);

        Task RevokeRefreshTokenAsync(string refreshToken);

        /// <summary>
        /// Logout the user by revoking both the access token and refresh token.
        /// </summary>
        /// <param name="accessToken">The access token, should be retrieved from Authorization header</param>
        /// <param name="refreshToken">The refresh token, should be retrieved from cookie</param>
        /// <returns>A Task representing the asynchronous logout operation.</returns>
        Task Logout(string accessToken, string refreshToken);
    }

    public class UserAppAppService(IAppRepository<User, Guid> userRepository,
                                   IUserMongoRepository userMongoRepository,
                                   IAppRepository<Organization, Guid> organizationRepository,
                                   IBlacklistedTokenMongoRepository blacklistedTokenRepository,
                                   IOrgMongoRepository orgMongoRepository,
                                   JwtService jwtService,
                                   IConfiguration configuration,
                                   IAppRepository<Role, int> roleRepository,
                                   IHttpContextAccessor http,
                                   ILogger<UserAppAppService> logger,
                                   IUserManager userManager) : AppServiceBase(userManager), IUserAppService
    {
        public async Task<AppResponse> GetAllUsers(PageRequest page)
        {
            var query = userRepository.Find(filter: u => !u.Deleted,
                                            include: [nameof(User.Roles), nameof(User.Organizations)]);
            var users = await query
                              .OrderBy(page.Sort)
                              .ToPagedListAsync(page.Number, page.Size);
            return AppResponse.SuccessResponse(users.MapPagedList(x => x.ToDisplayDto()));
        }

        public async Task<UserDisplayDto> CreateUser(UserInputDto userDto)
        {
            if (userDto == null)
                throw new Exception("Invalid user input");

            if (await ExistUsername(userDto.Username))
                throw new Exception("Username has already been taken");

            var user = userDto.ToEntity();
            var roles = await FindAllRoles(userDto.Roles);

            if (roles.Count > 0)
            {
                user.Roles.UnionWith(roles);
            }

            var created = await userRepository.CreateAsync(user);

            await userMongoRepository.InsertUser(await MapToMongo(created));

            return created.ToDisplayDto();
        }

        public async Task<AuthenticationResponse> Authenticate(UserLoginDto login)
        {
            var stopWatch = Stopwatch.StartNew();
            var foundUser = await FindUserByUserName(login.Username);
            bool passwordMatch = false;

            Console.WriteLine($"found user in db took: {stopWatch.ElapsedMilliseconds} ms");

            if (foundUser.User is not null)
            {
                passwordMatch = login.Password.PasswordVerify(foundUser.User.Password);
            }
            else
            {
                return new AuthenticationResponse
                {
                    Message = "Invalid username or password."
                };
            }

            if (!passwordMatch)
            {
                await LoginFailureHandler(foundUser.User);
                return new AuthenticationResponse
                {
                    Message = "Invalid username or password."
                };
            }

            if (foundUser.User.Locked)
            {
                return new AuthenticationResponse
                {
                    Message = "Too many failed attempts. Your account has been locked."
                };
            }

            if (foundUser.User is { LogInFailedCount: > 0, Locked: false }) await ResetAccount(foundUser.User);
            ;

            var orgId = string.Empty;
            var orgLists = foundUser.User.Organizations.Select(o => o.Id).ToList();

            //Check if OrgId was passed from client. If so, use it as working org.
            if (!string.IsNullOrEmpty(login.OrgId) && Guid.TryParse(login.OrgId, out Guid id))
            {
                if (orgLists.Contains(id)) //if orgId matches one of the user's orgs, use that as working org.
                {
                    orgId = id.ToString();
                }
            }

            //If no working org was specified, determine which org should be used based on last working org.
            if (foundUser.User.LastWorkingOrg is not null && string.IsNullOrEmpty(orgId))
            {
                if (orgLists.Contains(foundUser.User.LastWorkingOrg.Value))
                {
                    orgId = foundUser.User.LastWorkingOrg.Value.ToString();
                }
            }

            //If no working org could be determined, default to first org in list. If none available, set to empty string.
            if (string.IsNullOrEmpty(orgId))
                orgId = foundUser.User.Organizations.FirstOrDefault()?.Id.ToString() ?? string.Empty;

            var issuedAt = DateTime.UtcNow.ToLocalTime();
            //TODO: add user's permissions to token.
            var token = await jwtService.GenerateTokenAsync(foundUser.User, foundUser.Permisions, issuedAt, orgId);
            var org = string.IsNullOrEmpty(orgId)
                ? null
                : foundUser.User.Organizations.FirstOrDefault(o => o.Id.ToString() == orgId);
            Console.WriteLine($"Generate jwt took: {stopWatch.ElapsedMilliseconds} ms");
            return new AuthenticationResponse
            {
                Success = true,
                Message = "Success",
                Username = foundUser.User.Username,
                Id = foundUser.User.Id,
                AccessToken = token.AccessToken,
                RefreshToken = token.RefreshToken,
                IssueAt = issuedAt,
                ExpireAt = jwtService.GetExpiration(token.AccessToken),
                WorkingOrgId = orgId,
                WorkingTaxId = org is null ? string.Empty : org.TaxId,
                WorkingOrgShortName = org is null ? string.Empty : org.ShortName,
                WorkingOrgFullName = org is null ? string.Empty : org.FullName,
            };
        }

        public async Task<AuthenticationResponse> RefreshTokenAsync(string currentRefreshToken)
        {
            try
            {
                var user = await userRepository.Find(u => u.Id.ToString() == UserId, nameof(User.Roles))
                                               .FirstOrDefaultAsync();
                if (user is null)
                {
                    return new AuthenticationResponse { Success = false, Message = "User not found" };
                }

                var reloadRole = user.Roles.Select(r => r.RoleName).ToList();
                //generate new tokens
                (string newAccessToken, string newRefreshToken) =
                    await jwtService.RefreshTokenAsync(currentRefreshToken, reloadRole);

                return new AuthenticationResponse
                {
                    Success = true,
                    Message = "Success",
                    Username = jwtService.GetUsernameFromToken(newAccessToken),
                    Id = Guid.Parse(jwtService.GetUsernameFromToken(newAccessToken)),
                    AccessToken = newAccessToken,
                    RefreshToken = newRefreshToken,
                    IssueAt = jwtService.GetIssuedAt(newAccessToken),
                    ExpireAt = jwtService.GetExpiration(newAccessToken)
                };
            }
            catch (Exception e)
            {
                logger.LogError("Error: {message}, caused by {exceptionType}", e.Message, e.GetType().Name);
                return new AuthenticationResponse { Success = false, Message = "Failed to exchange tokens" };
            }
        }

        public async Task RevokeRefreshTokenAsync(string refreshToken)
        {
            await jwtService.RevokeRefreshTokenAsync(refreshToken); //revoke previous refresh token
        }

        public async Task Logout(string accessToken, string refreshToken)
        {
            var expDate = jwtService.GetExpiration(accessToken);
            await blacklistedTokenRepository
                .AddTokenToBlackList(accessToken, expDate); //add revoked access token to blacklist
            await RevokeRefreshTokenAsync(refreshToken);
        }

        public async Task<AppResponse> SelfChangePassword(string oldPassword, string newPassword)
        {
            try
            {
                var id = UserManager.CurrentUserId(); //get current user id
                if (id is null)
                    return AppResponse.Error("Unauthorized access");

                var user = await userRepository.FindByIdAsync(Guid.Parse(id));
                if (user is null)
                    return AppResponse.Error("User not found");

                var checkOldPassword = oldPassword.PasswordVerify(user.Password);
                if (!checkOldPassword)
                    return AppResponse.Error("Invalid old password");

                user.Password = newPassword.BCryptHash();
                await userRepository.UpdateAsync(user);
                return AppResponse.Ok("Password changed successfully");
            }
            catch (Exception e)
            {
                logger.LogError("Error: {message}, caused by {exceptionType}", e.Message, e.GetType().Name);
                return AppResponse.Error("Failed to update password");
            }
        }

        

        public async Task<AppResponse> ChangeUserRoles(Guid id, List<int> roleIds)
        {
            var user = await userRepository.Find(u => u.Id == id, ClaimTypes.Role).FirstOrDefaultAsync();
            if (user is null) return new AppResponse() { Success = false, Message = "User not found" };
            var roles = await roleRepository.Find(r => roleIds.Contains(r.Id)).ToListAsync();
            if (roles.Count == 0) return new AppResponse { Success = false, Message = "Role not found" };
            user.Roles.Clear();
            user.Roles = roles.ToHashSet();
            await userRepository.UpdateAsync(user);
            await UpdateUserWithMongo(user);
            return new AppResponse { Message = "OK" };
        }

        public async Task UnlockUser(Guid userId)
        {
            var user = await userRepository.Find(u => u.Id == userId).FirstOrDefaultAsync();
            if (user is null) throw new Exception("User not found");
            await ResetAccount(user);
            await userRepository.UpdateAsync(user);
        }


        public async Task<bool> ExistUsername(string username)
        {
            return await userRepository.ExistAsync(user => user.Username == username);
        }

        public async Task<(User? User, ISet<string> Permisions)> FindUserByUserName(string username)
        {
            var user = await userRepository.Find(u => u.Username == username && !u.Deleted,
                                                 include: [nameof(User.Organizations), nameof(User.Roles)])
                                           .FirstOrDefaultAsync();
            HashSet<string> userPermissions = [];
            //Get user permissions if user exists
            if (user is not null && user.Roles.Count > 0)
            {
                userPermissions = user.Roles.SelectMany(r => r.Permissions.Where(p => !p.Deleted))
                                      .Select(p => p.PermissionName).ToHashSet();
            }

            return (user, userPermissions);
        }

        public async Task<List<Role>> FindAllRoles(ICollection<int> roleIds)
        {
            return await roleRepository.Find(r => roleIds.Contains(r.Id)).ToListAsync();
        }

        public async Task<List<RoleDisplayDto>> FindRolesByUser(Guid userId)
        {
            try
            {
                var user = await userRepository.FindByIdAsync(userId);
                if (user is null) throw new Exception("User not found");
                var roles = await roleRepository.Find(r => r.Users.Contains(user))
                                                .ToListAsync();
                return roles.MapCollection(r => r.ToDisplayDto()).ToList();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public async Task<AppResponse> AddOrganizationToUser(Guid userId, ICollection<Guid> orgIds)
        {
            var user = await userRepository.Find(x => x.Id == userId, include: nameof(User.Organizations))
                                           .FirstOrDefaultAsync();
            if (user is null) return AppResponse.Error404("User not found");
            var org = await organizationRepository.Find(x => orgIds.Contains(x.Id)).ToListAsync();
            user.Organizations = org.ToHashSet();
            await userRepository.UpdateAsync(user);
            return AppResponse.Ok();
        }

        public async Task<AuthenticationResponse> ChangeWorkingOrganization(string orgId, string refreshToken)
        {
            //verify user is logged in.
            if (UserId is null) throw new Exception("Unauthorized access");

            //verify organization exists and user is a member of the organization:
            var org = await organizationRepository.Find(filter: x => x.Id.ToString() == orgId
                                                                     && x.Users.Any(u => u.Id.ToString() == UserId),
                                                        include: nameof(Organization.Users))
                                                  .FirstOrDefaultAsync();
            if (org is null)
                return new AuthenticationResponse
                {
                    Message = "Orgianization does not exist or you are not a member of this organization."
                };

            var username = UserManager.CurrentUsername()!;
            //var permissions = await GetUserPermissions(Guid.Parse(UserId));
            
            var roles = await GetUserRoles(Guid.Parse(UserId)); //update user's roles
            //Update new claims:
            var newClaims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, UserId),
                new Claim(JwtRegisteredClaimNames.Name, username),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("tenantId", string.Empty),
                new Claim("orgId", orgId),
                //new Claim("permissions", string.Join(",", permissions)),
                new Claim(ClaimTypes.Role, string.Join(",", roles))
            };
            var issuedAt = DateTime.UtcNow.ToLocalTime();
            var token = jwtService.GenerateTokenFromClaims(newClaims, issuedAt);

            return new AuthenticationResponse
            {
                Success = true,
                Message = "Success",
                Username = UserName,
                Id = Guid.Parse(UserId),
                AccessToken = token,
                IssueAt = issuedAt,
                RefreshToken = refreshToken,
                ExpireAt = jwtService.GetExpiration(token),
                WorkingOrgId = orgId,
                WorkingTaxId = org.TaxId,
                WorkingOrgShortName = org.ShortName,
                WorkingOrgFullName = org.FullName
            };
        }

        private async Task UpdateUserWithMongo(User user)
        {
            var userDoc = await MapToMongo(user);
            await userMongoRepository.UpdateUser(userDoc);
        }


        /// <summary>
        /// Retrieves the list of permissions for a user by their ID.
        /// </summary>
        /// <param name="id">The ID of the user.</param>
        /// <returns>A list of permission names associated with the user.</returns>
        private async Task<List<string>> GetUserPermissions(Guid id)
        {
            return await userRepository.Find(u => u.Id == id && !u.Deleted)
                                       .Include(u => u.Roles)
                                       .ThenInclude(r => r.Permissions)
                                       .SelectMany(u => u.Roles).Where(r => !r.Deleted)
                                       .SelectMany(r => r.Permissions).Where(p => !p.Deleted)
                                       .Select(p => p.PermissionName)
                                       .Distinct()
                                       .ToListAsync();
        }
        
        private async Task<ISet<string>> GetUserRoles(Guid uId)
        {
            var roles = await userRepository.Find(u => u.Id == uId, include: nameof(User.Roles))
                                            .SelectMany(u => u.Roles).Where(r => !r.Deleted)
                                            .Select(r => r.RoleName).Distinct()
                                            .ToListAsync();
            return roles.ToHashSet();
        }

        private async Task<UserDoc> MapToMongo(User user)
        {
            return new UserDoc
            {
                UserId = user.Id.ToString(),
                Permissions = (await GetUserPermissions(user.Id)).ToHashSet(),
            };
        }


        private async Task LoginFailureHandler(User user)
        {
            user.LogInFailedCount += 1; // count login attempt
            //Lock account if attempt reached limit
            if (user.LogInFailedCount == int.Parse(configuration["SecureLogin:FailedCountLimit"]!))
            {
                user.Locked = true;
            }

            await userRepository.UpdateAsync(user);
        }

        private async Task ResetAccount(User user)
        {
            user.LogInFailedCount = 0;
            user.Locked = false;
            await userRepository.UpdateAsync(user);
        }
    }
}