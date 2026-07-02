using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using UserMicroservices.Repository;
using UserMicroservices.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authorization;

namespace UserMicroservices.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly UserRepository _repo;
    private readonly IConfiguration _configuration;

 
    private static readonly ConcurrentDictionary<string, RefreshTokenInfo> _refreshTokens = new();

    public AuthController(UserRepository repo, IConfiguration configuration)
    {
        _repo = repo;
        _configuration = configuration;
    }

    [AllowAnonymous]
    [HttpPost("token")]
    public IActionResult Token([FromBody] LoginRequest request)
    {

        var user = _repo.GetUserByEmail(request.Email);
        if (user == null)
            return Unauthorized();

     
        if (user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTime.UtcNow)
        {
            return Forbid();
        }

        var hashed = System.Convert.ToBase64String(System.Security.Cryptography.SHA256.Create()
            .ComputeHash(Encoding.UTF8.GetBytes(request.Password ?? string.Empty)));

        if (!string.Equals(hashed, user.UserPassword, StringComparison.Ordinal))
        {
         
            user.FailedLoginAttempts++;
            if (user.FailedLoginAttempts >= 5)
            {
                user.LockoutEnd = DateTime.UtcNow.AddMinutes(15);
                user.FailedLoginAttempts = 0;
            }
            _repo.UpdateUserDetails(user);
            return Unauthorized();
        }

       
        user.FailedLoginAttempts = 0;
        user.LockoutEnd = null;
        _repo.UpdateUserDetails(user);

        var authResult = CreateTokens(user);

        return Ok(authResult);
    }

    [AllowAnonymous]
    [HttpPost("refresh")]
    public IActionResult Refresh([FromBody] RefreshRequest request)
    {
        if (string.IsNullOrEmpty(request.RefreshToken))
            return BadRequest();

        if (!_refreshTokens.TryGetValue(request.RefreshToken, out var info))
            return Unauthorized();

        if (info.ExpiresAt < DateTime.UtcNow)
        {
            _refreshTokens.TryRemove(request.RefreshToken, out _);
            return Unauthorized();
        }

        var user = _repo.GetUserByEmail(info.Email);
        if (user == null)
            return Unauthorized();

      
        _refreshTokens.TryRemove(request.RefreshToken, out _);
        var authResult = CreateTokens(user);

        return Ok(authResult);
    }

    private AuthResult CreateTokens(User user)
    {
        var jwtSection = _configuration.GetSection("Jwt");
        var key = jwtSection.GetValue<string>("Key") ?? "ThisIsASecretKeyForJwtTokenDoNotUseInProduction";
        var issuer = jwtSection.GetValue<string>("Issuer") ?? "MyKartIssuer";
        var audience = jwtSection.GetValue<string>("Audience") ?? "MyKartAudience";

        var tokenHandler = new JwtSecurityTokenHandler();
        var keyBytes = Encoding.UTF8.GetBytes(key);
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.EmailId),
            new Claim(ClaimTypes.Role, user.RoleName ?? string.Empty)
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(15),
            Issuer = issuer,
            Audience = audience,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(keyBytes), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        var tokenString = tokenHandler.WriteToken(token);

       
        var refreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        var refreshInfo = new RefreshTokenInfo
        {
            Email = user.EmailId,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };
        _refreshTokens[refreshToken] = refreshInfo;

        return new AuthResult
        {
            AccessToken = tokenString,
            RefreshToken = refreshToken,
            ExpiresAt = tokenDescriptor.Expires ?? DateTime.UtcNow.AddMinutes(15)
        };
    }
}


public record LoginRequest(string Email, string Password);
public record RefreshRequest(string RefreshToken);

public class AuthResult
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
}

public class RefreshTokenInfo
{
    public string Email { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
}
