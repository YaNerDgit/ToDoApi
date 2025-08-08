using Microsoft.AspNetCore.Mvc;
using ToDoApi.Contracts;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ToDoApi.Data;
using ToDoApi.Models;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.AspNetCore.Authorization;

namespace ToDoApi.Controllers;

public class AuthController(AppDbContext context, IConfiguration config) : ControllerBase
{
    [HttpPost("/api/auth/register")]
    public async Task<IActionResult> Register(RegisterRequest request, CancellationToken cancellationToken)
    {
        var existingUser = await context.Users.FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);
        if (existingUser != null)
        {
            return BadRequest("Email already exists");
        }

        var tempUser = new User();
        var passwordHasher = new PasswordHasher<User>();
        var hashedPassword = passwordHasher.HashPassword(tempUser, request.Password);

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            PasswordHash = hashedPassword,
            FullName = request.FullName,
            CreatedAt = DateTime.UtcNow,
            Role = UserRole.User
        };

        await context.Users.AddAsync(user, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        return Created();
    }

    [HttpPost("/api/auth/login")]
    public async Task<IActionResult> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        var existingUser = await context.Users.FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);
        if (existingUser == null)
        {
            return BadRequest("Email does not exist");
        }

        var passwordHasher = new PasswordHasher<User>();
        var passwordVerificationResult =
            passwordHasher.VerifyHashedPassword(existingUser, existingUser.PasswordHash, request.Password);
        if (passwordVerificationResult == PasswordVerificationResult.Failed)
        {
            return Unauthorized("Invalid credentials");
        }

        var jwtToken = await GenerateJwtToken(existingUser);
        return Ok(new LoginResponse(jwtToken));
    }

    private async Task<string> GenerateJwtToken(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.FullName),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
        };

        var token = new JwtSecurityToken(
            issuer: config["Jwt:Issuer"],
            audience: config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.Now.AddMinutes(30),
            signingCredentials: creds
        );

        string jwtToken = new JwtSecurityTokenHandler().WriteToken(token);
        return jwtToken;
    }
}