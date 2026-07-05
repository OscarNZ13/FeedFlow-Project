using FF_Api.ViewModels;
using FF_Business;
using FF_ModelsDB.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace FF_Api.Controllers;

[Route("[controller]")]
[ApiController]
public class UserApiController : ControllerBase
{
    private readonly IUserBusiness _userBusiness;

    public UserApiController(IUserBusiness userBusiness)
    {
        _userBusiness = userBusiness;
    }

    //Para identificar si el usuario es admin o sea que tengo RoleId= 1
    private bool IsAdmin()
    {
        var roleId = User.Claims.FirstOrDefault(c => c.Type == "roleId")?.Value;
        return roleId == "1";
    }

    //GET de usuarios
    [Authorize]
    [HttpGet("get-all")]
    public async Task<IActionResult> GetAll()
    {
        var roleId = User.Claims.FirstOrDefault(c => c.Type == "roleId")?.Value;
        if (roleId != "1") return Forbid("Solo administradores pueden ver usuarios.");

        var users = await _userBusiness.GetAllAsync();
        return Ok(users.Select(u => new {
            u.Id,
            u.Username,
            u.Email,
            u.RoleId
        }));
    }

    //GET de usuario por id
    [Authorize]
    [HttpGet("get-by-id/{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var roleId = User.Claims.FirstOrDefault(c => c.Type == "roleId")?.Value;
        if (roleId != "1") return Forbid("Solo administradores pueden ver usuarios.");

        var user = await _userBusiness.GetByIdAsync(id);
        if (user == null) return NotFound();

        return Ok(new
        {
            user.Id,
            user.Username,
            user.Email,
            user.RoleId
        });
    }


    //Crear usuario
    [HttpPost("create")]
    public async Task<IActionResult> Create([FromBody] CreateUserViewModel dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Username) || string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
            return BadRequest("Todos los campos son obligatorios.");

        var userByUsername = await _userBusiness.GetByUsernameAsync(dto.Username);
        if (userByUsername != null)
            return BadRequest("El nombre de usuario ya está en uso.");

        var userByEmail = await _userBusiness.GetByEmailAsync(dto.Email);
        if (userByEmail != null)
            return BadRequest("El correo electrónico ya está en uso.");

        var result = await _userBusiness.CreateAsync(dto.Username, dto.Email, dto.Password, dto.RoleId);

        return result ? Ok("Usuario creado.") : StatusCode(500, "Error al crear usuario.");
    }


    [Authorize]
    [HttpGet("check-availability")]
    public async Task<IActionResult> CheckAvailability(string username, string email)
    {
        var userByUsername = await _userBusiness.GetByUsernameAsync(username);
        if (userByUsername != null)
            return BadRequest("El nombre de usuario ya está en uso.");

        var userByEmail = await _userBusiness.GetByEmailAsync(email);
        if (userByEmail != null)
            return BadRequest("El correo electrónico ya está en uso.");

        return Ok("Disponible");
    }


    //Editar informacion usuario (user y correo)
    [HttpPut("edit-info/{id}")]
    public async Task<IActionResult> EditInfo(int id, [FromBody] EditInfoViewModel dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userByUsername = await _userBusiness.GetByUsernameAsync(dto.Username);
        if (userByUsername != null && userByUsername.Id != id)
            return BadRequest("El nombre de usuario ya está en uso.");

        var userByEmail = await _userBusiness.GetByEmailAsync(dto.Email);
        if (userByEmail != null && userByEmail.Id != id)
            return BadRequest("El correo electrónico ya está en uso.");

        var user = await _userBusiness.GetByIdAsync(id);
        if (user == null) return NotFound("Usuario no encontrado.");

        user.Username = dto.Username;
        user.Email = dto.Email;
        user.RoleId = dto.RoleId;

        var result = await _userBusiness.UpdateAsync(user);
        return result ? Ok("Usuario actualizado.") : StatusCode(500, "Error al actualizar usuario.");
    }


    [HttpGet("get-user/{id}")]
    public async Task<IActionResult> GetUser(int id)
    {
        var user = await _userBusiness.GetByIdAsync(id);
        if (user == null) return NotFound("Usuario no encontrado.");

        return Ok(new
        {
            user.Id,
            user.Username,
            user.Email,
            user.RoleId
        });
    }


    //Editar contraseña user
    [HttpPut("edit-password/{id}")]
    public async Task<IActionResult> EditPassword(int id, [FromBody] EditPasswordViewModel dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _userBusiness.UpdatePasswordAsync(id, dto.NewPassword);
        return result ? Ok("Contraseña actualizada.") : StatusCode(500, "Error al actualizar contraseña.");
    }



    //Eliminar usuario
    [Authorize]
    [HttpDelete("UserApi/{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var claim = User.FindFirst("id");
        if (claim == null)
            return Unauthorized("No se pudo identificar al usuario actual.");

        var currentUserId = int.Parse(claim.Value);
        if (id == currentUserId)
            return BadRequest("No puedes eliminar tu propio usuario.");

        var user = await _userBusiness.GetByIdAsync(id);
        if (user == null) return NotFound("Usuario no encontrado.");

        var result = await _userBusiness.DeleteUserAsync(id);
        return result ? Ok("Usuario eliminado correctamente.") : StatusCode(500, "Error al eliminar usuario.");
    }



    // POST: /UserApi/register
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterViewModel dto)
    {
        try
        {
            var result = await _userBusiness.RegisterAsync(dto.Username, dto.Email, dto.Password);
            if (!result) return BadRequest("No se pudo crear el usuario.");
            return Ok("Usuario registrado correctamente.");
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // POST: /UserApi/login
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginViewModel dto)
    {
        var token = await _userBusiness.LoginAsync(dto.Email, dto.Password);
        if (token == null) return Unauthorized("Credenciales incorrectas.");

        return Ok(new { token });
    }
}