using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using WebApp.Payloads;
using WebApp.Services.UserService;
using WebApp.Services.UserService.Dto;

namespace WebApp.Controllers;

[ApiController, Route("/api/auth")]
public class AuthController(IUserAppService userAppService,
                            IPermissionAppService permissionAppService) : ControllerBase
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
            return Ok(res);
        }

        if (res.TwoStepVerificationRequired)
        {
            return Ok(res);
        }

        // Add refresh token to the response cookies:
        if (res.RefreshToken is not null)
        {
            Response.Cookies.Append("refreshToken", res.RefreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Path = "/",
                Expires = res.ExpireAt!.Value.AddDays(30)
            });
        }

        // Remove refresh token from the response body:
        res.RefreshToken = null;
        return Ok(res);
    }

    [HttpPost("two-step-login")]
    public async Task<IActionResult> TwoStepLogin(UserLoginWithVerifyCodeDto login)
    {
        var result = await userAppService.Authenticate2Step(login);
        if (!result.Success)
        {
            return Ok(result);
        }

        // Add refresh token to the response cookies:
        if (result.RefreshToken is not null)
            Response.Cookies.Append("refreshToken", result.RefreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Path = "/",
                Expires = result.ExpireAt!.Value.AddDays(30)
            });
        // Remove refresh token from the response body:
        result.RefreshToken = null;
        return Ok(result);
    }

    /// <summary>
    /// Allow user to create a new verification code.
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    [HttpPost("send-verify-code/{key}")] [EnableRateLimiting("VerificationCodeLimit")]
    public async Task<IActionResult> RefreshVerificationCode([FromRoute] string key)
    {
        var result = await userAppService.RefreshVerificationCode(key);
        return Ok(result);
    }

    /// <summary>
    /// Refresh access token when expired
    /// </summary>
    /// <returns>The renew access token</returns>
    [HttpPost("refresh")] [Authorize]
    public async Task<IActionResult> RefreshToken()
    {
        var refreshToken = Request.Cookies["refreshToken"];
        if (string.IsNullOrEmpty(refreshToken))
        {
            return Unauthorized("Missing token.");
        }

        var res = await userAppService.RefreshTokenAsync(refreshToken);
        // Add new refresh token to the response cookies:
        Response.Cookies.Append("refreshToken", res.RefreshToken!, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None,
            Path = "/",
            Expires = res.ExpireAt!.Value.AddDays(30)
        });
        res.RefreshToken = null; // remove refresh token from the response body
        return Ok(res);
    }

    /// <summary>
    /// Sign out
    /// </summary>
    /// <returns></returns>
    [HttpPost("logout")] [Authorize]
    public async Task<IActionResult> Logout()
    {
        // Get tokens:
        var accessToken = HttpContext.Request.Headers.Authorization.ToString().Replace("Bearer ", "");
        var refreshToken = Request.Cookies["refreshToken"];
        // Check for missing tokens:
        if (string.IsNullOrEmpty(refreshToken) || string.IsNullOrEmpty(accessToken))
        {
            return Unauthorized("Missing tokens.");
        }

        // Call service:
        await userAppService.Logout(accessToken, refreshToken);
        return Ok();
    }

    /// <summary>
    /// Change working organization
    /// </summary>
    /// <param name="request">A request payload that contain the organization ID to change to.</param>
    /// <returns>The new access token and refresh token</returns>
    [HttpPost("change-org")] [Authorize]
    public async Task<IActionResult> ChangeWorkingOrganization([FromBody] ChangeOrgRequest request)
    {
        if (string.IsNullOrEmpty(request.OrgId))
        {
            return BadRequest("You must provide an org id.");
        }

        // Get current refresh token:
        var refreshToken = Request.Cookies["refreshToken"];
        if (string.IsNullOrEmpty(refreshToken))
        {
            return Unauthorized("Missing refresh token.");
        }

        // Call service:
        var res = await userAppService.ChangeWorkingOrganization(request.OrgId, refreshToken);
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

    [HttpGet("menu")]
    public async Task<IActionResult> LoadMenuItem()
    {
        var menuItems = await permissionAppService.GetMenuItems();
        return Ok(menuItems);
    }

    /// <summary>
    /// Authenticates a user and initiates the pre-login process using the provided credentials.
    /// </summary>
    /// <param name="userLogin">An object containing the user's login credentials and related information. Cannot be null.</param>
    /// <returns>An <see cref="IActionResult"/> containing the result of the pre-login operation. Returns a success response with
    /// authentication details if the credentials are valid; otherwise, returns an unauthorized response.</returns>
    [HttpPost("pre-login"), AllowAnonymous]
    public async Task<IActionResult> PreLogin(UserLoginDto userLogin)
    {
        try
        {
            var result = await userAppService.PreLogin(userLogin);
            return Ok(result);
        }
        catch (AuthenticationFailureException e)
        {
            return Unauthorized(e.Message);
        }
    }

    [HttpPost("final-login"), AllowAnonymous]
    public async Task<IActionResult> FinalLogin(FinalLoginDto login)
    {
        var result = await userAppService.FinalLogin(login);
        return Ok(result);
    }

    /// <summary>
    /// Invalidate the authentication code if user cancel the login process.
    /// </summary>
    /// <param name="id">The identifier of authentication code</param>
    /// <returns></returns>
    [HttpPut("invalidate-request/{id}"), AllowAnonymous]
    public async Task<IActionResult> InvalidateAuthenCode(string id)
    {
        await userAppService.InvalidateAuthCode(id);
        return Ok();
    }

    /// <summary>
    /// Verifies a user's credentials and two-step authentication code as part of the login process.
    /// </summary>
    /// <param name="login">An object containing the user's login credentials and two-step verification code. Cannot be null.</param>
    /// <returns>An <see cref="IActionResult"/> containing the result of the two-step login verification. Returns a success
    /// response if verification is successful; otherwise, returns an error response indicating the reason for failure.</returns>
    [HttpPost("verify-2step-login"), AllowAnonymous]
    public async Task<IActionResult> Verify2StepLogin(Verify2StepLoginDto login)
    {
        var result = await userAppService.Verify2StepLogin(login);
        return Ok(result);
    }
}