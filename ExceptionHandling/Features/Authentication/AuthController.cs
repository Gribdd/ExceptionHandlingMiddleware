using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ExceptionHandling.Database;
using ExceptionHandling.Database.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace ExceptionHandling.Features.Authentication;

[AllowAnonymous]
[Route("api/auth")]
[ApiController]
public class AuthController(ApplicationDbContext context,
    IConfiguration configuration,
    CancellationToken cancellationToken = default) : ControllerBase
{
    // POST: api/auth/login
    /// <summary>
    /// Authenticates a user by email and issues a JWT access token if the user exists.
    /// </summary>
    /// <param name="request"></param>
    /// <returns>
    /// 200 OK with a JWT token if authentication succeeds, 
    /// 401 Unauthorized if the email is invalid.
    /// </returns>
    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginUserRequest request)
    {
        var user = await context.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

        if (user == null)
        {
            return Unauthorized(new { Message = "Invalid email." });
        }


        // Generate JWT token
        var token = GenerateJwtToken(user);

        return Ok(new { Token = token });
    }

    private string GenerateJwtToken(User user)
    {
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(configuration["AuthConfiguration:Key"]!)
        );

        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()), 
            new Claim(JwtRegisteredClaimNames.Email, user.Email), 
        };

        var token = new JwtSecurityToken(
            issuer: configuration["AuthConfiguration:Issuer"],
            audience: configuration["AuthConfiguration:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
