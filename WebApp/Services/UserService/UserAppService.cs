using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Linq.Dynamic.Core;
using System.Security.Claims;
using EFCoreSecondLevelCacheInterceptor;
using Microsoft.EntityFrameworkCore;
using NanoidDotNet;
using Spire.Xls;
using WebApp.Core.Data;
using WebApp.Core.DomainEntities;
using WebApp.Enums;
using WebApp.Mongo.DocumentModel;
using WebApp.Mongo.MongoRepositories;
using WebApp.Payloads;
using WebApp.Payloads.AuthenticationPayloads;
using WebApp.Repositories;
using WebApp.Services.CommonService;
using WebApp.Services.EmailService;
using WebApp.Services.Mappers;
using WebApp.Services.OrganizationService.Dto;
using WebApp.Services.UserService.Dto;
using WebApp.Utils;
using X.Extensions.PagedList.EF;

namespace WebApp.Services.UserService
{
    public interface IUserAppService
    {
        Task<ResponseEntity> CreateUser(UserInputDto user);

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
        Task<ResponseEntity> GetAllUsers(PageRequest page);

        /// <summary>
        /// Unlocks a user account.
        /// </summary>
        /// <param name="userId">The ID of the user to unlock.</param>
        Task<ResponseEntity> LockOrUnlockUser(Guid userId);

        /// <summary>
        /// Changes the roles of a user.
        /// </summary>
        /// <param name="id">The ID of the user.</param>
        /// <param name="roleIds">The list of role IDs to assign to the user.</param>
        /// <returns>A response indicating the result of the operation.</returns>
        Task<ResponseEntity> ChangeUserRoles(Guid id, List<int> roleIds);

        /// <summary>
        /// Allows a user to change their own password.
        /// </summary>
        /// <param name="oldPassword">The current password of the user.</param>
        /// <param name="newPassword">The new password to set.</param>
        /// <returns>A response indicating the result of the operation.</returns>
        Task<ResponseEntity> SelfChangePassword(string oldPassword, string newPassword);

        /// <summary>
        /// Adds organizations to a user.
        /// </summary>
        /// <param name="user">The ID of the user.</param>
        /// <param name="orgIds">The collection of organization IDs to add to the user.</param>
        /// <returns>A response indicating the result of the operation.</returns>
        Task<ResponseEntity> AddOrganizationToUser(Guid user, ICollection<Guid> orgIds);

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

        /// <summary>
        /// Retrieves a list of all users for use in another service.
        /// </summary>
        /// <returns>An <see cref="ResponseEntity"/> containing the list of users.</returns>
        Task<ResponseEntity> GetAllUserForOtherService();

        Task<ResponseEntity> FindUserById(Guid id);
        Task<ResponseEntity> ResetPassword(Guid id, string newPassword);
        Task<ResponseEntity> UpdateBasicUserInfo(Guid userId, UserBasicInfoDto input);
        Task<AuthenticationResponse> Authenticate2Step(UserLoginWithVerifyCodeDto login);
        Task<AuthenticationResponse> RefreshVerificationCode(string key);
        Task<PreLoginResult> PreLogin(UserLoginDto input);
        Task<AuthenticationResponse> FinalLogin(FinalLoginDto input);
        Task InvalidateAuthCode(string id);
        Task<Verify2StepResponse> Verify2StepLogin(Verify2StepLoginDto input);
    }

    public partial class UserAppService(AppDbContext dbContext,
                                           IAppRepository<User, Guid> userRepository,
                                           IUserMongoRepository userMongoRepository,
                                           ILockedUserMongoRepository lockRepository,
                                           IAppRepository<Organization, Guid> organizationRepository,
                                           IBlacklistedTokenMongoRepository blacklistedTokenRepository,
                                           IAppRepository<UserVerification, long> userVerificationRepository,
                                           JwtService jwtService,
                                           IEmailAppService emailService,
                                           IConfiguration configuration,
                                           IAppRepository<Role, int> roleRepository,
                                           IHttpContextAccessor http,
                                           ILogger<UserAppService> logger,
                                           IUserManager userManager) : BaseAppService(userManager), IUserAppService
    {
        private readonly string _defaultEmail = "ketoan.sline@gmail.com";

        public async Task<ResponseEntity> GetAllUsers(PageRequest page)
        {
            try
            {
                var query = dbContext.Users.Where(u => !u.Deleted)
                                     .Include(x => x.Roles.Where(r => !r.Deleted))
                                     .Include(x => x.Organizations.Where(o => !o.Deleted));

                var pagedResult = await query
                                        .OrderBy(page.Sort)
                                        .ToPagedListAsync(page.Page, page.Size);
                
                var dtoResult = pagedResult.MapPagedList(x => x.ToDisplayDto());
                return page.Fields.Length > 0
                    ? ResponseEntity.OkResult(dtoResult.ProjectPagedList(page.Fields))
                    : ResponseEntity.OkResult(dtoResult);
            }
            catch (Exception ex)
            {
                logger.LogError("Error in GetAllUsers: {Message}, caused by {ExceptionType}", ex.Message,
                                ex.GetType().Name);
                return ResponseEntity.Error("Failed to retrieve users");
            }
        }


        public async Task<ResponseEntity> FindUserById(Guid id)
        {
            var foundUser = await dbContext.Users.Where(u => u.Id == id && !u.Deleted)
                                                .Include(u => u.Roles)
                                                .Include(u => u.Organizations.Where(o => !o.Deleted))
                                                .AsSplitQuery()
                                                .AsNoTracking()
                                                .FirstOrDefaultAsync();
            return foundUser is null
                ? ResponseEntity.Error404("User not found")
                : ResponseEntity.OkResult(foundUser.ToDisplayDto());
        }

        public async Task<ResponseEntity> GetAllUserForOtherService()
        {
            try
            {
                var users = await userRepository.Find(filter: u => !u.Deleted)
                                                .Select(x => new UserInfoDto
                                                {
                                                    Id = x.Id,
                                                    Username = x.Username
                                                })
                                                .OrderBy(x => x.Id)
                                                .ToListAsync();
                return ResponseEntity.OkResult(users);
            }
            catch (Exception ex)
            {
                logger.LogError("Error in GetAllUserForOtherService: {Message}, caused by {ExceptionType}", ex.Message,
                                ex.GetType().Name);
                return ResponseEntity.Error("Failed to retrieve users for other service");
            }
        }

        public async Task<ResponseEntity> CreateUser(UserInputDto userDto)
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

            if (userDto.Organizations != null && userDto.Organizations.Count > 0)
            {
                var org = await organizationRepository.Find(o => userDto.Organizations.Contains(o.Id)).ToListAsync();
                user.Organizations = org.ToHashSet();
            }

            var createdUser = await userRepository.CreateAsync(user);

            //await userMongoRepository.InsertUser(MapToMongo(createdUser));

            return ResponseEntity.OkResult(createdUser.ToDisplayDto());
        }

        public async Task<AuthenticationResponse> Authenticate(UserLoginDto login)
        {
            var verifyPasswordResult = await VerifyUserPassword(login.Username, login.Password);

            if (!verifyPasswordResult.IsValid)
            {
                return new AuthenticationResponse
                {
                    Success = false,
                    Message = verifyPasswordResult.Message,
                };
            }

            var foundUser = await FindUserByUserName(login.Username);

            //reset failed login attempts if user is not locked
            if (foundUser.User is { LogInFailedCount: > 0, Locked: false })
                await ResetLockCount(foundUser.User);

            if (foundUser.User!.VerificationRequired)
            {
                var verificator = await CreateVerificationCode(foundUser.User.Id,
                                                               foundUser.User.Username,
                                                               foundUser.User.Email ??
                                                               _defaultEmail); //fallback to default email if user has not registered an email yet
                await SendVerificationCodeEmail(verificator);
                return new AuthenticationResponse
                {
                    Success = true,
                    AccessToken = verificator.VerificationKey,
                    TwoStepVerificationRequired = true,
                    Id = verifyPasswordResult.Id!.Value,
                    Username = verificator.Username
                };
            }

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

        public async Task<AuthenticationResponse> Authenticate2Step(UserLoginWithVerifyCodeDto login)
        {
            var loginResult = await VerifyUser2StepLogin(login);
            if (!loginResult.IsValid || loginResult.Code is null)
            {
                return new AuthenticationResponse
                {
                    Success = false,
                    Message = "Mã xác thực không đúng hoặc đã quá hạn. Vui lòng thử lại."
                };
            }

            var issuedAt = DateTime.UtcNow.ToLocalTime();

            var user = await userRepository.Find(x => x.Id == login.UserId)
                                           .Include(x => x.Organizations)
                                           .Include(u => u.Roles)
                                           .ThenInclude(r => r.Permissions)
                                           .AsSplitQuery()
                                           .AsNoTracking()
                                           .FirstOrDefaultAsync();

            if (user is null)
            {
                return new AuthenticationResponse
                {
                    Success = false,
                    Message = "User không tồn tại."
                };
            }

            await InvalidateVerificationCode(user.Id);
            var orgId = string.Empty;
            var workingOrg = user.Organizations.FirstOrDefault(o => o.Id == user.LastWorkingOrg)
                             ?? user.Organizations.FirstOrDefault();
            var permissions = user.Roles.SelectMany(r => r.Permissions.Where(p => !p.Deleted))
                                  .Select(p => p.PermissionName).ToHashSet();
            //generate new tokens
            var token = await jwtService.GenerateTokenAsync(user, permissions, issuedAt, orgId);
            return new AuthenticationResponse
            {
                Success = true,
                Message = "Success",
                Username = user.Username,
                Id = user.Id,
                AccessToken = token.AccessToken,
                RefreshToken = token.RefreshToken,
                IssueAt = issuedAt,
                ExpireAt = jwtService.GetExpiration(token.AccessToken),
                WorkingOrgId = workingOrg is null ? string.Empty : workingOrg.Id.ToString(),
                WorkingTaxId = workingOrg is null ? string.Empty : workingOrg.TaxId,
                WorkingOrgShortName = workingOrg is null ? string.Empty : workingOrg.ShortName,
                WorkingOrgFullName = workingOrg is null ? string.Empty : workingOrg.FullName,
            };
        }

        public async Task<AuthenticationResponse> RefreshVerificationCode(string key)
        {
            var verificator = await userVerificationRepository.Find(x => x.VerificationKey == key)
                                                              .FirstOrDefaultAsync();
            if (verificator is null)
                return new AuthenticationResponse { Success = false, Message = "Failed to refresh your code" };


            //create new verification code
            var newVerificator =
                await CreateVerificationCode(verificator.UserId, verificator.Username, verificator.Email);
            //remove old verification code
            await userVerificationRepository.HardDeleteAsync(verificator.Id);
            await SendVerificationCodeEmail(newVerificator);
            return new AuthenticationResponse
            {
                Success = true,
                AccessToken = newVerificator.VerificationKey,
                TwoStepVerificationRequired = true,
                Id = newVerificator.UserId,
                Username = newVerificator.Username
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

        public async Task<ResponseEntity> SelfChangePassword(string oldPassword, string newPassword)
        {
            try
            {
                var id = UserManager.CurrentUserId(); //get current user id
                if (id is null)
                    return ResponseEntity.Error("Unauthorized access");

                var user = await userRepository.FindByIdAsync(Guid.Parse(id));
                if (user is null)
                    return ResponseEntity.Error("User not found");

                var checkOldPassword = oldPassword.PasswordVerify(user.Password);
                if (!checkOldPassword)
                    return ResponseEntity.Error("Invalid old password");

                user.Password = newPassword.BCryptHash();
                await userRepository.UpdateAsync(user);
                return ResponseEntity.Ok("Password changed successfully");
            }
            catch (Exception e)
            {
                logger.LogError("Error: {message}, caused by {exceptionType}", e.Message, e.GetType().Name);
                return ResponseEntity.Error("Failed to update password");
            }
        }

        public async Task<ResponseEntity> UpdateBasicUserInfo(Guid userId, UserBasicInfoDto input)
        {
            var found = await userRepository.Find(u => !u.Deleted && u.Id == userId).FirstOrDefaultAsync();
            if (found is null) return ResponseEntity.Error404("User not found");
            found.FullName = input.FullName;
            found.Email = input.Email;
            await userRepository.UpdateAsync(found);
            return ResponseEntity.Ok();
        }

        public async Task<ResponseEntity> ResetPassword(Guid id, string newPassword)
        {
            try
            {
                var foundUser = await userRepository.Find(u => u.Id == id && !u.Deleted)
                                                    .FirstOrDefaultAsync();
                if (foundUser is null)
                    return ResponseEntity.Error404("User not found");
                if (newPassword.Length < 6)
                    return ResponseEntity.Error400("Password must be at least 6 characters long");
                foundUser.Password = newPassword.BCryptHash();
                await userRepository.UpdateAsync(foundUser);
                return ResponseEntity.Ok("Password changed successfully");
            }
            catch (Exception e)
            {
                logger.LogError("Error: {message}, caused by {exceptionType}", e.Message, e.GetType().Name);
                return ResponseEntity.Error("Error while resetting password: " + e.Message);
            }
        }


        public async Task<ResponseEntity> ChangeUserRoles(Guid id, List<int> roleIds)
        {
            var user = await userRepository.Find(u => u.Id == id)
                                           .Include(u => u.Roles)
                                           .FirstOrDefaultAsync();
            if (user is null) return new ResponseEntity() { Success = false, Message = "User not found" };
            var roles = await roleRepository.Find(r => roleIds.Contains(r.Id)).ToListAsync();
            if (roles.Count == 0) return new ResponseEntity { Success = false, Message = "Role not found" };
            user.Roles.Clear();
            user.Roles = roles.ToHashSet();
            await userRepository.UpdateAsync(user);
            // await UpdateUserWithMongo(user);
            return new ResponseEntity { Message = "OK" };
        }

        public async Task<ResponseEntity> LockOrUnlockUser(Guid userId)
        {
            var user = await userRepository.Find(u => u.Id == userId).FirstOrDefaultAsync();
            if (user is null) throw new Exception("User not found");
            if (user.Id.ToString() == UserId)
                return ResponseEntity.Error400("Quản trị viên không được phép khóa/mở khóa tài khoản của chính mình");
            user.Locked = !user.Locked; // toggle lock status
            await userRepository.UpdateAsync(user);

            if (user.Locked)
            {
                await LockInMongo(user.Id); // lock user in MongoDB
            }
            else
            {
                await UnlockInMongo(user.Id); // unlock user in MongoDB
            }

            var message = user.Locked ? $"Khóa tài khoản {user.Username}" : $"Mở khóa tài khoản {user.Username}";
            return ResponseEntity.Ok(message);
        }


        public async Task<bool> ExistUsername(string username)
        {
            username = username.RemoveSpace() ?? string.Empty;
            return await userRepository.ExistAsync(user => user.Username == username);
        }

        public async Task<(User? User, ISet<string> Permisions)> FindUserByUserName(string username)
        {
            var foundUser = await dbContext.Users
                                           .Include(u => u.Roles)
                                           .ThenInclude(r => r.Permissions)
                                           .Include(u => u.Organizations)
                                           .AsSplitQuery()
                                           .AsNoTracking()
                                           .Cacheable()
                                           .FirstOrDefaultAsync(u => u.Username == username);

            HashSet<string> userPermissions = [];
            //Get user permissions if user exists
            if (foundUser is not null && foundUser.Roles.Count > 0)
            {
                userPermissions = foundUser.Roles.SelectMany(r => r.Permissions.Where(p => !p.Deleted))
                                           .Select(p => p.PermissionName).ToHashSet();
            }

            return (foundUser, userPermissions);
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

        public async Task<ResponseEntity> AddOrganizationToUser(Guid userId, ICollection<Guid> orgIds)
        {
            var user = await dbContext.Users
                                      .Include(u => u.Organizations)
                                      .FirstOrDefaultAsync(x => x.Id == userId);

            if (user is null) return ResponseEntity.Error404("User not found");
            var org = await organizationRepository.Find(x => orgIds.Contains(x.Id)).ToListAsync();
            user.Organizations = org.ToHashSet();
            await userRepository.UpdateAsync(user);
            return ResponseEntity.Ok();
        }

        public async Task<AuthenticationResponse> ChangeWorkingOrganization(string orgId, string refreshToken)
        {
            //verify user is logged in.
            if (UserId is null) throw new Exception("Unauthorized access");

            //verify organization exists and user is a member of the organization:
            var foundOrg = await dbContext.Organizations
                                          .Include(o => o.Users)
                                          .FirstOrDefaultAsync(o => o.Id == orgId.ToGuid()
                                                                    && o.Users.Any(u => u.Id == UserId.ToGuid()));

            if (foundOrg is null)
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
                WorkingTaxId = foundOrg.TaxId,
                WorkingOrgShortName = foundOrg.ShortName,
                WorkingOrgFullName = foundOrg.FullName
            };
        }

        private async Task UpdateUserWithMongo(User user)
        {
            var userDoc = MapToMongo(user);
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
                                       .AsNoTracking()
                                       .AsSplitQuery()
                                       .ToListAsync();
        }

        private async Task<ISet<string>> GetUserRoles(Guid uId)
        {
            var roles = await dbContext.Users.Where(u => u.Id == uId)
                                       .Include(u => u.Roles)
                                       .SelectMany(u => u.Roles).Where(r => !r.Deleted)
                                       .Select(r => r.RoleName).Distinct()
                                       .AsNoTracking()
                                       .AsSplitQuery()
                                       .ToHashSetAsync();
            return roles;
        }

        private UserDoc MapToMongo(User user)
        {
            return new UserDoc
            {
                UserId = user.Id.ToString(),
                //Permissions = (await GetUserPermissions(user.Id)).ToHashSet(),
                Locked = user.Locked
            };
        }


        /// <summary>
        /// Handles user login failures by incrementing the failed login attempt count
        /// and locking the account if the failed attempts reach a predefined limit.
        /// </summary>
        /// <param name="user">The user for whom the login failure is being handled.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
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

        private async Task ResetLockCount(User user)
        {
            user.LogInFailedCount = 0;
            user.Locked = false;
            await userRepository.UpdateAsync(user);
        }

        private async Task LockInMongo(Guid userId)
        {
            await lockRepository.LockUser(userId);
        }

        private async Task UnlockInMongo(Guid userId)
        {
            await lockRepository.UnlockUser(userId);
        }

        /// <summary>
        /// Create verification code for 2nd step login.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="username"></param>
        /// <param name="email"></param>
        /// <returns></returns>
        private async Task<UserVerification> CreateVerificationCode(Guid userId, string username, string email)
        {
            var verificationKey = Ulid.NewUlid().ToString();
            var verificationCode = StringExtension.RandomNumber(length: 4);
            var limitTime = configuration["SecureLogin:VerificationCodeLifetime"] ?? "120";
            var expirationTime = DateTime.Now.AddSeconds(int.Parse(limitTime));
            var code = new UserVerification
            {
                VerificationKey = verificationKey,
                VerificationCode = verificationCode,
                VerificationCodeExpiration = expirationTime,
                UserId = userId,
                Username = username,
                Email = email
            };
            return await userVerificationRepository.CreateAsync(code);
        }

        /// <summary>
        /// Validate verification code from user input.
        /// </summary>
        /// <param name="login"></param>
        /// <returns></returns>
        private async Task<(bool IsValid, UserVerification? Code)> VerifyUser2StepLogin(UserLoginWithVerifyCodeDto login)
        {
            var verificationCode = await dbContext.UserVerifications
                                                  .Where(c => c.UserId == login.UserId &&
                                                              c.VerificationCode == login.Code &&
                                                              c.VerificationKey == login.Key)
                                                  .AsNoTracking()
                                                  .FirstOrDefaultAsync();
            if (verificationCode is null || verificationCode.VerificationCodeExpiration <= DateTime.Now)
                return (false, verificationCode);
            return (true, verificationCode);
        }

        /// <summary>
        /// Remove the verification code after user has used it.
        /// </summary>
        /// <param name="userId"></param>
        private async Task InvalidateVerificationCode(Guid userId)
        {
            var ids = await userVerificationRepository.Find(x => x.UserId == userId)
                                                      .Select(x => x.Id)
                                                      .ToListAsync();
            await userVerificationRepository.HardDeleteManyAsync(ids);
        }

        private async Task SendVerificationCodeEmail(UserVerification verificator)
        {
            var emailBody = $"""
                             <div style='font-family: Arial, sans-serif;'>
                             <p>Tài khoản <b>{verificator.Username}</b> của bạn có 1 yêu cầu xác thực đăng nhập vào lúc {DateTime.Now:dd/MM/yyyy HH:mm:ss}.</p><br/>
                             <p>Mã xác thực của bạn là: <b style='color:red; font-size:25px'>{verificator.VerificationCode}<b></p><br/>
                             <p>Hãy sử dụng mã này trong vòng 2 phút để đăng nhập vào SLINE.</p>
                             <p>Hãy bỏ qua tin nhắn này nếu bạn không phải là người yêu cầu mã xác thực.</p>
                             <p>Vui lòng không trả lời email này. Xin cảm ơn!</p>
                             <div>
                             """;
            await emailService.SendEmailAsync("SLINE - Mã xác thực", emailBody, verificator.Email);
        }

        private async Task<(bool IsValid, string? Message, Guid? Id)> VerifyUserPassword(
            string username, string password)
        {
            var user = await userRepository.Find(x => x.Username == username && !x.Deleted)
                                           .FirstOrDefaultAsync();
            if (user is null) return (false, "Tài khoản hoặc mật khẩu không đúng.", null);
            var checkpass = password.PasswordVerify(user.Password);
            if (!checkpass)
            {
                await LoginFailureHandler(user);
                return (false, "Tài khoản hoặc mật khẩu không đúng.", null);
            }

            if (user.Locked) return (false, "Tài khoản của bạn đã bị khóa.", null);
            return (true, null, user.Id);
        }
    }
}