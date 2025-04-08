using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApp.Payloads;
using WebApp.Services.UserService;
using WebApp.Services.UserService.Dto;

namespace WebApp.Controllers;

[ApiController, Route("/api/auth")]
public class AuthController(IUserAppService userAppService) : ControllerBase
{
    /// <summary>
    /// Authenticate user
    /// </summary>
    /// <returns>The access token.</returns>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login(UserLoginDto login)
    {
        var res = await userAppService.Authenticate(login);

        if (!res.Success)
        {
            return Unauthorized(res);
        }
        
        // Add refresh token to the response cookies:
        Response.Cookies.Append("refreshToken", res.RefreshToken!, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None,
            Path = "/",
            Expires = res.ExpireAt!.Value.AddDays(30)
        });
        // Remove refresh token from the response body:
        res.RefreshToken = null;
        return Ok(res);

    }
    /// <summary>
    /// Refresh access token when expired
    /// </summary>
    /// <returns>The renew access token</returns>
    [HttpPost("refresh")][Authorize]
    public async Task<IActionResult> RefreshToken()
    {
        var refreshToken = Request.Cookies["refreshToken"];
        if (string.IsNullOrEmpty(refreshToken))
        {
            return Unauthorized("Missing token.");
        }
        var res = await userAppService.RefreshTokenAsync(refreshToken);
        Response.Cookies.Append("refreshToken", res.RefreshToken!, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None,
            Path = "/",
            Expires = res.ExpireAt!.Value.AddDays(30)
        });
        res.RefreshToken = null;
        return Ok(res);
    }

    /// <summary>
    /// Sign out
    /// </summary>
    /// <returns></returns>
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var refreshToken = Request.Cookies["refreshToken"];
        if (string.IsNullOrEmpty(refreshToken))
        {
            return Unauthorized("Missing refresh token.");
        }
        await userAppService.RevokeRefreshTokenAsync(refreshToken);
        return Ok();
    }

    /// <summary>
    /// Change working organization
    /// </summary>
    /// <param name="request">A request payload that contain the organization ID to change to.</param>
    /// <returns>The new access token and refresh token</returns>
    [HttpPost("change-org")]
    [Authorize]
    public async Task<IActionResult> ChangeWorkingOrganization([FromBody] ChangeOrgRequest request)
    {
        if (string.IsNullOrEmpty(request.OrgId))
        {
            return BadRequest("You must provide an org id.");
        }
        var res = await userAppService.ChangeWorkingOrganization(request.OrgId);
        if (!res.Success)
        {
            return BadRequest(res);
        }

        // Add new refresh token to the response cookies:
        Response.Cookies.Append("refreshToken", res.RefreshToken!, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None,
            Path = "/",
            Expires = res.ExpireAt!.Value.AddDays(30)
        });
        // Remove refresh token from the response body:
        res.RefreshToken = null;
        return Ok(res);
    }
}