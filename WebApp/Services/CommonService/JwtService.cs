using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Security;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using WebApp.Core.DomainEntities;
using WebApp.Mongo.DocumentModel;
using WebApp.Mongo.MongoRepositories;
using JwtRegisteredClaimNames = System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames;

namespace WebApp.Services.CommonService;

public class JwtService(IConfiguration config, 
                        IRefreshTokenMongoRepository refreshTokenRepository, 
                        IHttpContextAccessor httpContextAccessor)
{
    private readonly string _secretKey = config["JwtSettings:SecretKey"]!;
    private readonly string _issuer = config["JwtSettings:Issuer"]!;
    private readonly string _audience = config["JwtSettings:Audience"]!;
    private readonly int _expiryMinutes = int.Parse(config["JwtSettings:ExpiryMinutes"]!);
    private readonly JwtSecurityTokenHandler _tokenHandler = new();

    public async Task<(string AccessToken, string RefreshToken)> GenerateTokenAsync(User user, DateTime issuedAt, string? orgId = null)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Name, user.Username),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim("tenantId", string.Empty),
            new Claim("orgId", orgId ?? string.Empty)
        };
        var refreshToken = GenerateRefreshToken();
        var refreshTokenDoc = new RefreshTokenDoc
        {
            Token = refreshToken,
            UserId = user.Id.ToString(),
            IssuedAt = issuedAt,
            ExpiresAt = issuedAt.AddDays(365),
            IsRevoked = false
        };
        await refreshTokenRepository.CreateTokenAsync(refreshTokenDoc);
        return (GenerateTokenFromClaims(claims, issuedAt), refreshToken);
    }

    public async Task<(string AccessToken, string RefreshToken)> RefreshTokenAsync(string refreshToken)
    {
        var tokenEntity = await refreshTokenRepository.FindTokenAsync(refreshToken);
        if (tokenEntity == null || tokenEntity.IsRevoked || tokenEntity.ExpiresAt < DateTime.UtcNow)
        {
            throw new SecurityTokenException("Invalid or expired refresh token.");
        }
        var oldAccessToken = httpContextAccessor.HttpContext?.Request.Headers.Authorization.FirstOrDefault()?.Split(" ")[1];
        if (string.IsNullOrEmpty(oldAccessToken))
        {
            throw new SecurityTokenException("No access token found in the request header.");
        }
        var claims = GetClaimsFromToken(oldAccessToken).ToList();
        var userIdClaim = claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out Guid userId) || tokenEntity.UserId != userId.ToString())
        {
            throw new SecurityTokenException("Token mismatched.");
        }

        var issuedAt = DateTime.UtcNow;
        claims.RemoveAll(c => c.Type == JwtRegisteredClaimNames.Jti);
        claims.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));
        
        var newAccessToken = GenerateTokenFromClaims(claims, issuedAt);
        
        await refreshTokenRepository.RevokeTokenAsync(refreshToken, GetClientIp());
        var newRefreshToken = GenerateRefreshToken();
        var newRefreshTokenEntity = new RefreshTokenDoc()
        {
            Token = newRefreshToken,
            UserId = userId.ToString(),
            IssuedAt = issuedAt,
            ExpiresAt = issuedAt.AddDays(365),
            IsRevoked = false
        };
        await refreshTokenRepository.CreateTokenAsync(newRefreshTokenEntity);

        return (newAccessToken, newRefreshToken);
    }

    public async Task RevokeTokenAsync(string refreshToken)
    {
        var refreshTokenDoc = await refreshTokenRepository.FindTokenAsync(refreshToken);
        if (refreshTokenDoc is null) throw new SecurityException("Refresh token not found");
        await refreshTokenRepository.RevokeTokenAsync(refreshToken, GetClientIp());
    }
    
    public async Task RevokeRefreshTokenAsync(string refreshToken)
    {
        var tokenEntity = await refreshTokenRepository.FindTokenAsync(refreshToken);
        if (tokenEntity == null)
        {
            throw new SecurityTokenException("Refresh token not found.");
        }
        await refreshTokenRepository.RevokeTokenAsync(refreshToken, GetClientIp());
    }
    
    public string GenerateTokenFromClaims(IEnumerable<Claim> claims, DateTime issuedAt)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);
        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            expires: issuedAt.AddMinutes(_expiryMinutes),
            signingCredentials: creds
        );
        return _tokenHandler.WriteToken(token);
    }

    public DateTime GetExpiration(string token)
    {
        var jwt = new JsonWebToken(token);
        return jwt.ValidTo.ToLocalTime();
    }
    
    public DateTime GetIssuedAt(string token)
    {
        var jwt = new JsonWebToken(token);
        return jwt.IssuedAt.ToLocalTime();
    }

    public string GetUsernameFromToken(string token)
    {
        var claims = GetClaimsFromToken(token);
        return claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)?.Value ?? string.Empty;
    }

    public string GetUserIdFromToken(string token)
    {
        var claims = GetClaimsFromToken(token);
        return claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Name)?.Value ?? string.Empty;
    }

    public IEnumerable<Claim> GetClaimsFromToken(string token)
    {
        var jwt = new JsonWebToken(token);
        return jwt.Claims;
    }

    private static string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create() ;
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    private string GetClientIp()
    {
        return httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "Unidentified IP";
    }
}