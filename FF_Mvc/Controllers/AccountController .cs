using FF_Mvc.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

namespace FF_Mvc.Controllers;

public class AccountController : Controller
{
    private readonly HttpClient _httpClient;
    private const string ApiBaseUrl = "";

    public AccountController()
    {
        var handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        };
        _httpClient = new HttpClient(handler);
    }

    // GET: /Account/Login
    public IActionResult Login()
    {
        return View();
    }

    // POST: /Account/Login
    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        
        var json = JsonSerializer.Serialize(model);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync($"{ApiBaseUrl}/login", content);

        if (!response.IsSuccessStatusCode)
        {
            ModelState.AddModelError("", "Credenciales incorrectas.");
            return View(model);
        }

        var responseBody = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(responseBody);
        var token = result.GetProperty("token").GetString();

        // Guardar token en sesión
        HttpContext.Session.SetString("JwtToken", token!);

        return RedirectToAction("Index", "Home");
    }

    // GET: /Account/Register
    public IActionResult Register()
    {
        return View();
    }

    // POST: /Account/Register
    [HttpPost]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        
        var json = JsonSerializer.Serialize(model);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync($"{ApiBaseUrl}/register", content);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            ModelState.AddModelError("", error);
            return View(model);
        }

        return RedirectToAction("Login");
    }

    // GET: /Account/Logout
    public IActionResult Logout()
    {
        HttpContext.Session.Remove("JwtToken");
        return RedirectToAction("Login");
    }
}