using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using UserMicroservices.Repository;
using UserMicroservices.Models;

namespace UserMicroservices.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly UserRepository _repo;
    private readonly IConfiguration _configuration;

    public AuthController(UserRepository repo, IConfiguration configuration)
    {
        _repo = repo;
        _configuration = configuration;
    }

    [HttpPost("token")]
    public IActionResult Token([FromBody] LoginRequest request)
    {

        var user = _repo.GetUserByEmail(request.Email);
        if (user == null)
            return Unauthorized();

        // Check lockout
        if (user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTime.UtcNow)
        {
            return Forbid(); // user is locked out
        }

        // Compare hashed passwords
        var hashed = System.Convert.ToBase64String(System.Security.Cryptography.SHA256.Create()
            .ComputeHash(Encoding.UTF8.GetBytes(request.Password ?? string.Empty)));

        if (!string.Equals(hashed, user.UserPassword, StringComparison.Ordinal))
        {
            // increment failed attempts
            user.FailedLoginAttempts++;
            if (user.FailedLoginAttempts >= 5)
            {
                user.LockoutEnd = DateTime.UtcNow.AddMinutes(15);
                user.FailedLoginAttempts = 0;
            }
            _repo.UpdateUserDetails(user);
           // return Unauthorized();
        }

        // reset failed attempts on success
        user.FailedLoginAttempts = 0;
        user.LockoutEnd = null;
        _repo.UpdateUserDetails(user);

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
            Expires = DateTime.UtcNow.AddHours(2),
            Issuer = issuer,
            Audience = audience,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(keyBytes), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        var tokenString = tokenHandler.WriteToken(token);

        return Ok(new { token = tokenString });
    }
}

public record LoginRequest(string Email, string Password);
