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
public class AuthController :ControllerBase
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
        Console.WriteLine("=== TOKEN REQUEST STARTED ===");
        Console.WriteLine($"[1] Request Email: {request.Email}");
        Console.WriteLine($"[2] Timestamp: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss.fff}");

        var user = _repo.GetUserByEmail(request.Email);
        Console.WriteLine($"[3] User Found: {(user != null ? "YES" : "NO")}");
        if (user == null)
        {
            Console.WriteLine("[4] ❌ User not found - Returning Unauthorized");
            return Unauthorized();
        }

        Console.WriteLine($"[5] User Email: {user.EmailId}");
        Console.WriteLine($"[6] User Role: {user.RoleName}");
        Console.WriteLine($"[7] Account Lockout Status: {(user.LockoutEnd.HasValue ? $"LOCKED until {user.LockoutEnd:yyyy-MM-dd HH:mm:ss}" : "NOT LOCKED")}");

        if (user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTime.UtcNow)
        {
            Console.WriteLine("[8] ❌ Account is locked - Returning Forbidden");
            return Forbid();
        }

        Console.WriteLine("[9] Validating password...");
        var hashed = System.Convert.ToBase64String(System.Security.Cryptography.SHA256.Create()
            .ComputeHash(Encoding.UTF8.GetBytes(request.Password ?? string.Empty)));

        Console.WriteLine($"[10] Password Match: {(string.Equals(hashed, user.UserPassword, StringComparison.Ordinal) ? "✓ MATCH" : "✗ MISMATCH")}");

        if (!string.Equals(hashed, user.UserPassword, StringComparison.Ordinal))
        {
            Console.WriteLine("[11] ❌ Password incorrect");
            user.FailedLoginAttempts++;
            Console.WriteLine($"[12] Failed Login Attempts: {user.FailedLoginAttempts}");

            if (user.FailedLoginAttempts >= 5)
            {
                user.LockoutEnd = DateTime.UtcNow.AddMinutes(15);
                user.FailedLoginAttempts = 0;
                Console.WriteLine($"[13] ⚠️  Account locked until: {user.LockoutEnd:yyyy-MM-dd HH:mm:ss}");
            }
            _repo.UpdateUserDetails(user);
            return Unauthorized();
        }

        Console.WriteLine("[14] ✅ Password verified successfully");
        user.FailedLoginAttempts = 0;
        user.LockoutEnd = null;
        _repo.UpdateUserDetails(user);
        Console.WriteLine("[15] Reset failed login attempts to 0");

        Console.WriteLine("[16] Calling CreateTokens method...");
        var authResult = CreateTokens(user);
        Console.WriteLine("[17] CreateTokens returned successfully");

        Console.WriteLine("=== TOKEN REQUEST COMPLETED ===\n");
        return Ok(authResult);
    }

    [AllowAnonymous]
    [HttpPost("refresh")]
    public IActionResult Refresh([FromBody] RefreshRequest request)
    {
        Console.WriteLine("\n=== REFRESH TOKEN REQUEST STARTED ===");
        Console.WriteLine($"[1] Refresh Token Received (length: {request.RefreshToken?.Length ?? 0})");
        Console.WriteLine($"[2] Timestamp: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss.fff}");

        if (string.IsNullOrEmpty(request.RefreshToken))
        {
            Console.WriteLine("[3] ❌ Refresh token is empty or null");
            return BadRequest();
        }

        Console.WriteLine("[4] Checking if refresh token exists in cache...");
        if (!_refreshTokens.TryGetValue(request.RefreshToken, out var info))
        {
            Console.WriteLine("[5] ❌ Refresh token not found in cache");
            return Unauthorized();
        }

        Console.WriteLine($"[6] ✓ Refresh token found in cache");
        Console.WriteLine($"[7] Token Email: {info.Email}");
        Console.WriteLine($"[8] Token Expires: {info.ExpiresAt:yyyy-MM-dd HH:mm:ss}");

        if (info.ExpiresAt < DateTime.UtcNow)
        {
            Console.WriteLine("[9] ❌ Refresh token expired");
            _refreshTokens.TryRemove(request.RefreshToken, out _);
            Console.WriteLine("[10] Expired token removed from cache");
            return Unauthorized();
        }

        Console.WriteLine("[11] ✓ Refresh token is still valid");

        var user = _repo.GetUserByEmail(info.Email);
        Console.WriteLine($"[12] User lookup: {(user != null ? "✓ FOUND" : "❌ NOT FOUND")}");
        if (user == null)
        {
            Console.WriteLine("[13] ❌ User not found by email");
            return Unauthorized();
        }

        Console.WriteLine($"[14] User Details - Email: {user.EmailId}, Role: {user.RoleName}");
        Console.WriteLine("[15] Removing old refresh token from cache...");
        _refreshTokens.TryRemove(request.RefreshToken, out _);
        Console.WriteLine("[16] ✓ Old token removed");

        Console.WriteLine("[17] Generating new tokens...");
        var authResult = CreateTokens(user);
        Console.WriteLine("[18] ✓ New tokens generated");

        Console.WriteLine("=== REFRESH TOKEN REQUEST COMPLETED ===\n");
        return Ok(authResult);
    }

    private AuthResult CreateTokens(User user)
    {
        Console.WriteLine("\n--- CREATE TOKENS METHOD STARTED ---");
        Console.WriteLine($"[A] User Email: {user.EmailId}");

        var jwtSection = _configuration.GetSection("Jwt");
        var key = jwtSection.GetValue<string>("Key") ?? "ThisIsASecretKeyForJwtTokenDoNotUseInProduction";
        var issuer = jwtSection.GetValue<string>("Issuer") ?? "MyKartIssuer";
        var audience = jwtSection.GetValue<string>("Audience") ?? "MyKartAudience";

        Console.WriteLine($"[B] JWT Key (first 10 chars): {key?.Substring(0, Math.Min(10, key.Length))}...");
        Console.WriteLine($"[C] JWT Issuer: {issuer}");
        Console.WriteLine($"[D] JWT Audience: {audience}");

        var tokenHandler = new JwtSecurityTokenHandler();
        Console.WriteLine("[E] JwtSecurityTokenHandler created");

        var keyBytes = Encoding.UTF8.GetBytes(key);
        Console.WriteLine($"[F] Key converted to bytes (length: {keyBytes.Length})");

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.EmailId),
            new Claim(ClaimTypes.Role, user.RoleName ?? string.Empty)
        };
        Console.WriteLine($"[G] Claims created:");
        Console.WriteLine($"    - Name (Email): {user.EmailId}");
        Console.WriteLine($"    - Role: {user.RoleName ?? "EMPTY"}");

        var expirationTime = DateTime.UtcNow.AddMinutes(15);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = expirationTime,
            Issuer = issuer,
            Audience = audience,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(keyBytes), SecurityAlgorithms.HmacSha256Signature)
        };
        Console.WriteLine($"[H] SecurityTokenDescriptor created");
        Console.WriteLine($"    - Expires: {expirationTime:yyyy-MM-dd HH:mm:ss}");
        Console.WriteLine($"    - Algorithm: HmacSha256Signature");

        var token = tokenHandler.CreateToken(tokenDescriptor);
        Console.WriteLine($"[I] JWT Token created (type: {token.GetType().Name})");

        var tokenString = tokenHandler.WriteToken(token);
        Console.WriteLine($"[J] Token serialized to string (length: {tokenString.Length})");
        Console.WriteLine($"[K] Token Preview: {tokenString.Substring(0, Math.Min(50, tokenString.Length))}...");

        // REFRESH TOKEN GENERATION
        Console.WriteLine("\n--- REFRESH TOKEN GENERATION ---");
        var refreshTokenBytes = RandomNumberGenerator.GetBytes(64);
        Console.WriteLine($"[L] Random bytes generated (64 bytes)");

        var refreshToken = Convert.ToBase64String(refreshTokenBytes);
        Console.WriteLine($"[M] Refresh token generated (Base64 encoded)");
        Console.WriteLine($"[N] Refresh Token length: {refreshToken.Length}");

        var refreshExpiresAt = DateTime.UtcNow.AddDays(7);
        var refreshInfo = new RefreshTokenInfo
        {
            Email = user.EmailId,
            ExpiresAt = refreshExpiresAt
        };
        Console.WriteLine($"[O] Refresh token info created:");
        Console.WriteLine($"    - Email: {user.EmailId}");
        Console.WriteLine($"    - Expires: {refreshExpiresAt:yyyy-MM-dd HH:mm:ss}");

        _refreshTokens[refreshToken] = refreshInfo;
        Console.WriteLine($"[P] Refresh token stored in cache (Total tokens in cache: {_refreshTokens.Count})");

        var authResult = new AuthResult
        {
            AccessToken = tokenString,
            RefreshToken = refreshToken,
            ExpiresAt = tokenDescriptor.Expires ?? DateTime.UtcNow.AddMinutes(15)
        };

        Console.WriteLine("\n--- AUTH RESULT SUMMARY ---");
        Console.WriteLine($"[Q] Access Token Generated:");
        Console.WriteLine($"    Length: {authResult.AccessToken.Length}");
        Console.WriteLine($"    Expires: {authResult.ExpiresAt:yyyy-MM-dd HH:mm:ss}");
        Console.WriteLine($"[R] Refresh Token Generated:");
        Console.WriteLine($"    Length: {authResult.RefreshToken.Length}");
        Console.WriteLine($"[S] Returning AuthResult...");
        Console.WriteLine("--- CREATE TOKENS METHOD COMPLETED ---\n");

        return authResult;
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
