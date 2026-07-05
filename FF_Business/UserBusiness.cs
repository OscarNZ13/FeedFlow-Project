using FF_ModelsDB.Models;
using FF_Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BC = BCrypt.Net.BCrypt;

namespace FF_Business;

public interface IUserBusiness
{
    Task<bool> RegisterAsync(string username, string email, string password);
    Task<string?> LoginAsync(string email, string password);
    //Para crear, editar, editar contraseña (admin) y eliminar usuarios (admin)
    Task<bool> CreateUserAsync(string username, string email, string password);

    Task<bool> CreateAsync(string username, string email, string password, int roleId);

    Task<bool> UpdateUserInfoAsync(int id, string username, string email);
    Task<bool> UpdatePasswordAsync(int id, string newPassword);
    Task<bool> DeleteUserAsync(int id);
    //Lisatado
    Task<List<User>> GetAllAsync();
    //Get por id
    Task<User?> GetByIdAsync(int id);
    //Para administrar role de usuarios
    Task<bool> UpdateUserRoleAsync(int id, int roleId);
    Task<User?> GetByUsernameAsync(string username);
    Task<User?> GetByEmailAsync(string email);
    Task<bool> UpdateAsync(User user);
    //Para encontrar por email
    Task<User?> FindByEmailAsync(string email);   

}

public class UserBusiness(IUserRepository userRepository, IConfiguration configuration) : IUserBusiness
{
    private readonly IUserRepository _userRepository = userRepository;
    private readonly IConfiguration _configuration = configuration;

    public async Task<bool> CreateAsync(string username, string email, string password, int roleId)
    {
        var user = new User
        {
            Username = username,
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password), 
            RoleId = roleId,
            CreatedAt = DateTime.UtcNow
        };

        return await _userRepository.CreateAsync(user);
    }

    public async Task<List<User>> GetAllAsync()
    {
        return await _userRepository.GetAllAsync();
    }

    public async Task<User?> GetByIdAsync(int id)
    {
        return await _userRepository.FindByIdAsync(id);
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        return await _userRepository.FindByUsernameAsync(username);
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _userRepository.FindByEmailAsync(email);
    }



    //Crear usuario
    public async Task<bool> CreateUserAsync(string username, string email, string password)
    {
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);
        var newUser = new User
        {
            Username = username,
            Email = email,
            PasswordHash = hashedPassword,
            RoleId = 2, 
            CreatedAt = DateTime.UtcNow
        };
        return await _userRepository.CreateAsync(newUser);
    }

    public async Task<bool> CreateAsync(User user)
    {
        return await _userRepository.CreateAsync(user);
    }


    //Actualizar rol de usuario
    public async Task<bool> UpdateUserRoleAsync(int id, int roleId)
    {
        var user = await _userRepository.FindByIdAsync(id);
        if (user == null) return false;

        user.RoleId = roleId;
        return await _userRepository.UpdateAsync(user);
    }

    public async Task<bool> UpdateAsync(User user)
    {
        return await _userRepository.UpdateAsync(user);
    }



    //Editar informacion usuario (user y correo)
    public async Task<bool> UpdateUserInfoAsync(int id, string username, string email)
    {
        var user = await _userRepository.FindByIdAsync(id);
        if (user == null) return false;

        user.Username = username;
        user.Email = email;

        return await _userRepository.UpdateAsync(user);
    }


    //Editar contraseña user
    public async Task<bool> UpdatePasswordAsync(int id, string newPassword)
    {
        var user = await _userRepository.FindByIdAsync(id);
        if (user == null) return false;

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        return await _userRepository.UpdateAsync(user);
    }

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


    public async Task<User?> FindByEmailAsync(string email)
    {
        return await _userRepository.FindByEmailAsync(email);
    }
    public async Task<string?> LoginAsync(string email, string password)
    {
        var user = await _userRepository.FindByEmailAsync(email);
        if (user == null) return null;

        if (!BC.Verify(password, user.PasswordHash))
            return null;

        return GenerateTokenWithId(user);
    }

    private string GenerateTokenWithId(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
        new Claim("id", user.Id.ToString()),              //Esto es para que delete funcione y detecte si el usuario que intenta eliminar es el mismo user logueado
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


    public async Task<bool> DeleteUserAsync(int id)
    {
        return await _userRepository.DeleteAsync(id);
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