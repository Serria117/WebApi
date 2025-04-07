using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Linq.Dynamic.Core;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
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
        Task<AuthenticationResponse> Authenticate(UserLoginDto login);
        Task<bool> ExistUsername(string username);
        Task<User?> FindUserByUserName(string username);
        Task<List<Role>> FindAllRoles(ICollection<int> roleIds);
        Task<List<RoleDisplayDto>> FindRolesByUser(Guid userId);
        Task<AppResponse> GetAllUsers(PageRequest page);
        Task UnlockUser(Guid userId);
        Task<AppResponse> ChangeUserRoles(Guid id, List<int> roleIds);
        Task<AppResponse> SelfChangePassword(string oldPassword, string newPassword);
        Task<AppResponse> AddOrganizationToUser(Guid user, ICollection<Guid> orgIds);
        public Task<AuthenticationResponse> ChangeWorkingOrganization(string orgId);
        Task<AuthenticationResponse> RefreshTokenAsync(string refreshToken);
        Task RevokeRefreshTokenAsync(string refreshToken);
        Task Logout(string accessToken);
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
                                   IUserManager userManager) : AppServiceBase(userManager), IUserAppService
    {
        public async Task<AppResponse> GetAllUsers(PageRequest page)
        {
            var query = userRepository.Find(u => !u.Deleted, "Roles");
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
            var user = await FindUserByUserName(login.Username);
            bool passwordMatch = false;

            Console.WriteLine($"found user in db took: {stopWatch.ElapsedMilliseconds} ms");

            if (user is not null)
            {
                passwordMatch = login.Password.PasswordVerify(user.Password);
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
                await LoginFailureHandler(user);
                return new AuthenticationResponse
                {
                    Message = "Invalid username or password."
                };
            }

            if (user.Locked)
            {
                return new AuthenticationResponse
                {
                    Message = "Your account has been locked."
                };
            }

            if (user is { LogInFailedCount: > 0, Locked: false }) await ResetAccount(user);
            var orgId = string.Empty;
            if (!string.IsNullOrEmpty(login.OrgId) && Guid.TryParse(login.OrgId, out Guid id))
            {
                if (await orgMongoRepository.ExistAsync(id.ToString())) ;
                {
                    orgId = id.ToString();
                }
            }

            var issuedAt = DateTime.UtcNow.ToLocalTime();
            var token = await jwtService.GenerateTokenAsync(user, issuedAt, orgId);
            Console.WriteLine($"Generate jwt took: {stopWatch.ElapsedMilliseconds} ms");
            return new AuthenticationResponse
            {
                Success = true,
                Message = "Success",
                Username = user.Username,
                Id = user.Id,
                AccessToken = token.AccessToken,
                RefreshToken = token.RefreshToken,
                IssueAt = issuedAt,
                ExpireAt = jwtService.GetExpiration(token.AccessToken)
            };
        }

        public async Task<AuthenticationResponse> RefreshTokenAsync(string refreshToken)
        {
            try
            {
                (string newAccessToken, string newRefreshToken) = await jwtService.RefreshTokenAsync(refreshToken);
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
                return new AuthenticationResponse(){Success = false, Message = e.Message };
            }
        }

        public async Task RevokeRefreshTokenAsync(string refreshToken)
        {
            await jwtService.RevokeRefreshTokenAsync(refreshToken);
        }

        public async Task Logout(string accessToken)
        {
            var expDate = jwtService.GetExpiration(accessToken); 
            await blacklistedTokenRepository.AddTokenToBlackList(accessToken, expDate);
        }

        public async Task<AppResponse> SelfChangePassword(string oldPassword, string newPassword)
        {
            var id = UserManager.CurrentUserId();
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

        public async Task<AppResponse> ChangeUserRoles(Guid id, List<int> roleIds)
        {
            var user = await userRepository.Find(u => u.Id == id, "Roles").FirstOrDefaultAsync();
            if (user is null) return new AppResponse() { Success = false, Message = "User not found" };
            var roles = await roleRepository.Find(r => roleIds.Contains(r.Id)).ToListAsync();
            if (roles.Count == 0) return new AppResponse { Success = false, Message = "Role not found" };
            user.Roles.Clear();
            user.Roles = roles.ToHashSet();
            await userRepository.UpdateAsync(user);
            await UpdateUserWithMongo(user);
            return new AppResponse() { Message = "OK" };
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

        public async Task<User?> FindUserByUserName(string username)
        {
            return await userRepository.Find(u => u.Username == username && !u.Deleted).FirstOrDefaultAsync();
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

        public async Task<AuthenticationResponse> ChangeWorkingOrganization(string orgId)
        {
            //verify user is logged in.
            if (UserId is null) throw new Exception("Unauthorized access");
            
            var refreshToken = http.HttpContext?.Request.Cookies["refreshToken"];
            
            //verify organization exists and user is a member of the organization:
            var _ = await organizationRepository.Find(filter: x => x.Id.ToString() == orgId
                                                                   && x.Users.Any(u => u.Id.ToString() == UserId),
                                                      include: nameof(Organization.Users))
                                                .AnyAsync();
            if (!_) throw new Exception("Organization not found");
            
            var username = UserManager.CurrentUsername()!;
            //Update new claims:
            var newClaims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, UserId),
                new Claim(JwtRegisteredClaimNames.Name, username),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("tenantId", string.Empty),
                new Claim("orgId", orgId)
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
                ExpireAt = jwtService.GetExpiration(token)
            };
        }

        public async Task UpdateUserWithMongo(Guid userId)
        {
            var user = await userRepository.FindByIdAsync(userId);
            if (user is not null)
            {
                var userDoc = await MapToMongo(user);
                await userMongoRepository.InsertUser(userDoc);
            }
        }

        private async Task UpdateUserWithMongo(User user)
        {
            var userDoc = await MapToMongo(user);
            await userMongoRepository.UpdateUser(userDoc);
        }

        private async Task<User?> FindUserByUsername(string name)
        {
            return await userRepository
                         .Find(x => x.Username == name && !x.Deleted)
                         .Include(u => u.Roles).ThenInclude(r => r.Permissions)
                         .FirstOrDefaultAsync();
        }

        private async Task<List<string>> GetUserPermissions(Guid id)
        {
            return await userRepository.Find(u => u.Id == id && !u.Deleted)
                                       .Include(u => u.Roles).ThenInclude(r => r.Permissions)
                                       .SelectMany(u => u.Roles)
                                       .SelectMany(r => r.Permissions).Select(p => p.PermissionName)
                                       .Distinct()
                                       .ToListAsync();
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