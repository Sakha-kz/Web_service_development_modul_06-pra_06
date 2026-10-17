using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using ProductSecurityApi.Models;

namespace ProductSecurityApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _configuration;

    public AuthController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        // Проверка учетных данных согласно заданию (student / 12345)
        if (request.Username != "student" || request.Password != "12345")
        {
            return Unauthorized(new
            {
                statusCode = StatusCodes.Status401Unauthorized,
                message = "Неверное имя пользователя или пароль."
            });
        }

        var jwtSettings = _configuration.GetSection("Jwt");
        var secretKey = jwtSettings["Key"] ?? "SuperSecretKeyForJwtAuthenticationModule06Pra06_MustBeLongEnough!";
        var issuer = jwtSettings["Issuer"] ?? "ProductSecurityApi";
        var audience = jwtSettings["Audience"] ?? "ProductSecurityClient";
        var expirationMinutes = int.TryParse(jwtSettings["ExpiresInMinutes"], out var mins) ? mins : 60;

        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(secretKey);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, request.Username),
                new Claim(ClaimTypes.Role, "Student"),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            }),
            Expires = DateTime.UtcNow.AddMinutes(expirationMinutes),
            Issuer = issuer,
            Audience = audience,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        var tokenString = tokenHandler.WriteToken(token);

        return Ok(new
        {
            token = tokenString,
            tokenType = "Bearer",
            expiresInMinutes = expirationMinutes,
            expiration = tokenDescriptor.Expires
        });
    }
}
