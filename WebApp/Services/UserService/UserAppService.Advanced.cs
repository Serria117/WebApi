using EFCoreSecondLevelCacheInterceptor;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using NanoidDotNet;
using System.Runtime.InteropServices;
using WebApp.Core.DomainEntities;
using WebApp.Enums;
using WebApp.Payloads;
using WebApp.Payloads.AuthenticationPayloads;
using WebApp.Services.CommonService;
using WebApp.Services.Mappers;
using WebApp.Services.OrganizationService.Dto;
using WebApp.Services.UserService.Dto;
using WebApp.Utils;
using Z.EntityFramework.Plus;

namespace WebApp.Services.UserService;

public partial class UserAppService
{
    public async Task<PreLoginResult> PreLogin(UserLoginDto input)
    {
        var user = await dbContext.Users
                                  .Include(u => u.Organizations.OrderByDescending(o => o.CreateAt))
                                  .Where(u => u.Username == input.Username && !u.Deleted)
                                  .AsSplitQuery()
                                  .Cacheable(CacheExpirationMode.Sliding, TimeSpan.FromMinutes(20))
                                  .FirstOrDefaultAsync();

        if (user == null)
            throw new AuthenticationFailureException("Username hoặc password không hợp lệ.");

        if (user.Locked)
        {
            throw new AuthenticationFailureException("Tài khoản của bạn đã bị khóa.");
        }

        var isPasswordValid = input.Password.PasswordVerify(user.Password);
        if (!isPasswordValid)
        {
            await LoginFailureHandler(user);
            throw new AuthenticationFailureException("Username hoặc password không hợp lệ.");
        }

        var orgList = user.Organizations.Select(o => new OrganizationSelectDto
        {
            Id = o.Id,
            ShortName = o.ShortName ?? string.Empty,
            FullName = o.FullName,
            TaxId = o.TaxId,
            UnsignName = o.UnsignName
        }).ToList();

        var newAuth = new UserAuthenticationToken
        {
            UserId = user.Id,
            Expiration = DateTime.UtcNow.ToLocalTime().AddMinutes(20),
            IsUsed = false
        };

        await dbContext.UserAuthenticationTokens.AddAsync(newAuth);

        //Check if user has 2-step verification enabled
        if (user.VerificationRequired)
        {
            var verificator = await CreateVerificationCode(user.Id, user.Username, user.Email ?? _defaultEmail);
            await SendVerificationCodeEmail(verificator);
            newAuth.Is2StepRequired = true; //Mark for 2-step check in final login step
            await dbContext.SaveChangesAsync();
            return new PreLoginResult
            {
                Id = user.Id,
                Username = user.Username,
                Success = true,
                AuthCode = newAuth.Id.ToString(),
                VerificationKey = verificator.VerificationKey,
                OrganizationList = orgList
            };
        }

        await dbContext.SaveChangesAsync();

        return new PreLoginResult
        {
            Id = user.Id,
            Success = true,
            AuthCode = newAuth.Id.ToString(),
            OrganizationList = orgList
        };
    }

    public async Task<Verify2StepResponse> Verify2StepLogin(Verify2StepLoginDto input)
    {
        if (input.AuthCode.IsNullOrEmpty() ||
            input.VerificationCode.IsNullOrEmpty() ||
            input.VerificationKey.IsNullOrEmpty())
        {
            return new Verify2StepResponse
            {
                Success = false,
                Message = "Bạn chưa nhập mã xác thực.",
                ResponseCode = AuthResponseCode.EmptyInput
            };
        }

        var authCode = await dbContext.UserAuthenticationTokens
                                      .Where(c => c.Id == input.AuthCode.ToGuid()
                                                 && !c.IsUsed
                                                 && c.Expiration >= DateTime.Now)
                                      .FirstOrDefaultAsync();

        if (authCode == null)
        {
            return new Verify2StepResponse
            {
                Success = false,
                Message = "Phiên làm việc không hợp lệ. Vui lòng thử lại.",
                ResponseCode = AuthResponseCode.AuthenticationTokenInvalid
            };
        }

        var verificator = await dbContext.UserVerifications
                                     .Where(c => c.UserId == input.UserId &&
                                                 c.VerificationCode == input.VerificationCode &&
                                                 c.VerificationKey == input.VerificationKey &&
                                                 c.VerificationCodeExpiration >= DateTime.Now)
                                     .AsNoTracking()
                                     .FirstOrDefaultAsync();
        if (verificator == null)
        {
            return new Verify2StepResponse
            {
                Success = false,
                Message = "Mã xác thực không hợp lệ hoặc đã quá hạn. Vui lòng thử lại.",
                ResponseCode = AuthResponseCode.VerificationCodeInvalid
            };
        }

        return new Verify2StepResponse
        {
            Success = true,
            Message = "Success",
            ResponseCode = AuthResponseCode.Success
        };
    }

    public async Task<AuthenticationResponse> FinalLogin(FinalLoginDto input)
    {
        var authCode = await dbContext.UserAuthenticationTokens
                                      .Where(c => c.Id == input.AuthCode.ToGuid()
                                                 && !c.IsUsed
                                                 && c.Expiration >= DateTime.Now)
                                      .FirstOrDefaultAsync();

        if (authCode == null)
        {
            return new AuthenticationResponse
            {
                Success = false,
                Message = "Phiên làm việc không hợp lệ. Vui lòng thử lại."
            };
        }

        if (authCode.Is2StepRequired)
        {
            var verificator = await dbContext.UserVerifications
                                             .Where(c => c.UserId == authCode.UserId &&
                                                         c.VerificationCode == input.VerificationCode &&
                                                         c.VerificationKey == input.VerificationKey)
                                             .FirstOrDefaultAsync();

            if (verificator == null || verificator.VerificationCodeExpiration < DateTime.UtcNow.ToLocalTime())
            {
                return new AuthenticationResponse
                {
                    Success = false,
                    Message = "Mã xác thực không hợp lệ hoặc đã quá hạn. Vui lòng thử lại."
                };
            }
            //After successful verification, remove the verification code so it cannot be used again.
            dbContext.UserVerifications.Remove(verificator);
            await dbContext.SaveChangesAsync();
        }

        var user = await dbContext.Users
                                  .Include(u => u.Organizations)
                                  .Include(u => u.Roles)
                                  .ThenInclude(r => r.Permissions)
                                  .Where(u => u.Id == authCode.UserId)
                                  .AsNoTracking()
                                  .AsSplitQuery()
                                  .Cacheable(CacheExpirationMode.Sliding, TimeSpan.FromDays(1))
                                  .FirstOrDefaultAsync();

        if (user == null)
        {
            return new AuthenticationResponse
            {
                Success = false,
                Message = "Không tìm thấy tài khoản."
            };
        }

        var org = user.Organizations.FirstOrDefault(o => o.Id == input.OrganizationId);

        if (org == null)
        {
            return new AuthenticationResponse
            {
                Success = false,
                Message = "Tài khoản không có quyền truy cập khách hàng."
            };
        }

        var issuedAt = DateTime.UtcNow.ToLocalTime();

        var (accessToken, refreshToken) = await jwtService
            .GenerateTokenAsync(user: user,
                                issuedAt: issuedAt,
                                orgId: input.OrganizationId.ToString(),
                                permissions: user.Roles
                                                 .SelectMany(r => r.Permissions)
                                                 .Select(p => p.PermissionName)
                                                 .ToHashSet());
        //After successful authentication, mark the authCode as used so it can no longer be used to login again.
        authCode.IsUsed = true;
        await dbContext.SaveChangesAsync();
        return new AuthenticationResponse
        {
            Success = true,
            Message = "Success",
            Username = user.Username,
            Id = user.Id,
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            IssueAt = issuedAt,
            ExpireAt = jwtService.GetExpiration(accessToken),
            WorkingOrgId = org.Id.ToString(),
            WorkingTaxId = org.TaxId,
            WorkingOrgShortName = org.ShortName,
            WorkingOrgFullName = org.FullName,
        };
    }

    public async Task InvalidateAuthCode(string id)
    {
        var code = await dbContext.UserAuthenticationTokens
                                  .FindAsync(id.ToGuid());
        if (code is not null)
        {
            code.IsUsed = true;
            await dbContext.SaveChangesAsync();
        }
    }
}
