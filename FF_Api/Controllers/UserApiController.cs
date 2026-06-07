using FF_Business;
using FF_Api.ViewModels;
using Microsoft.AspNetCore.Mvc;

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