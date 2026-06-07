using BC = BCrypt.Net.BCrypt;
using FF_ModelsDB.Models;
using FF_Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace FF_Business;

public interface IUserBusiness
{
    Task<bool> RegisterAsync(string username, string email, string password);
    Task<string?> LoginAsync(string email, string password);
}

public class UserBusiness(IUserRepository userRepository, IConfiguration configuration) : IUserBusiness
{
    private readonly IUserRepository _userRepository = userRepository;
    private readonly IConfiguration _configuration = configuration;

    public async Task<bool> RegisterAsync(string username, string email, string password)
    {
        var existingUsername = await _userRepository.FindByUsernameAsync(username);
        if (existingUsername != null)
            throw new Exception("El usuario ya existe.");

        var existingEmail = await _userRepository.FindByEmailAsync(email);
        if (existingEmail != null)
            throw new Exception("El email ya está registrado.");

        var hashedPassword = BC.HashPassword(password);

        var newUser = new User
        {
            Username = username,
            Email = email,
            PasswordHash = hashedPassword,
            RoleId = 1,
            CreatedAt = DateTime.UtcNow
        };

        return await _userRepository.CreateAsync(newUser);
    }

    public async Task<string?> LoginAsync(string email, string password)
    {
        var user = await _userRepository.FindByEmailAsync(email);
        if (user == null) return null;

        bool isValid = BC.Verify(password, user.PasswordHash);
        if (!isValid) return null;

        return GenerateToken(user);
    }

    private string GenerateToken(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim("username", user.Username),
            new Claim("email", user.Email),
            new Claim("roleId", user.RoleId.ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}