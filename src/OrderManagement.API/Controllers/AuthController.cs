using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace OrderManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _config;

    public AuthController(IConfiguration config)
    {
        _config = config;
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        // Simple hardcoded users for demo purposes
        var (role, valid) = request.Username?.ToLower() switch
        {
            "admin" when request.Password == "admin123" => ("Manager", true),
            "user" when request.Password == "user123" => ("User", true),
            _ => ("", false)
        };

        if (!valid)
            return Unauthorized(new { message = "Invalid username or password" });

        var token = GenerateToken(request.Username!, role);
        return Ok(new { token, role, username = request.Username });
    }

    private string GenerateToken(string username, string role)
    {
        var key = _config["Jwt:Key"]
            ?? Environment.GetEnvironmentVariable("JWT_KEY")
            ?? "ThisIsADevelopmentKeyThatIsLongEnough123!";

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, username),
            new Claim(ClaimTypes.Role, role)
        };

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"] ?? "OrderManagementAPI",
            audience: _config["Jwt:Audience"] ?? "OrderManagementClient",
            claims: claims,
            expires: DateTime.UtcNow.AddHours(2),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

public class LoginRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
