using FF_Mvc.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace FF_Mvc.Controllers;

public class UsersController : Controller
{
    private readonly HttpClient _httpClient;
    private const string ApiBaseUrl = "https://localhost:7283/UserApi";

    public UsersController(IHttpClientFactory factory)
    {
        _httpClient = factory.CreateClient();
    }

    private void AddJwt()
    {
        var token = HttpContext.Session.GetString("JwtToken");
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    //INDEX
    public async Task<IActionResult> Index()
    {
        AddJwt();
        var response = await _httpClient.GetAsync($"{ApiBaseUrl}/get-all");
        if (!response.IsSuccessStatusCode) return View(new List<UserViewModel>());

        var users = JsonSerializer.Deserialize<List<UserViewModel>>(
            await response.Content.ReadAsStringAsync(),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
        );

        return View(users);
    }


    public IActionResult Create() => View();

    [HttpPost]
    public async Task<IActionResult> Create(CreateUserViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        AddJwt();

        //Valida si el username o email ya existe (Se valida en API)
        var checkResponse = await _httpClient.GetAsync($"{ApiBaseUrl}/check-availability?username={model.Username}&email={model.Email}");
        if (!checkResponse.IsSuccessStatusCode)
        {
            var errorMessage = await checkResponse.Content.ReadAsStringAsync();

            if (errorMessage.Contains("usuario"))
                ModelState.AddModelError("Username", errorMessage);

            else if (errorMessage.Contains("correo"))
                ModelState.AddModelError("Email", errorMessage);

            else
                ModelState.AddModelError("", errorMessage);

            return View(model);
        }

        // Verificar que RoleId sea valido
        if (model.RoleId != 1 && model.RoleId != 2)
        {
            ModelState.AddModelError("RoleId", "Debe seleccionar un rol válido.");
            return View(model);
        }

        var json = JsonSerializer.Serialize(model);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync($"{ApiBaseUrl}/create", content);

        if (response.IsSuccessStatusCode)
            return RedirectToAction("Index");

        var error = await response.Content.ReadAsStringAsync();
        ModelState.AddModelError("", error);
        return View(model);
    }



    // GET: /Users/EditInfo/5
    public async Task<IActionResult> EditInfo(int id)
    {
        AddJwt();

        var response = await _httpClient.GetAsync($"{ApiBaseUrl}/get-user/{id}");
        if (!response.IsSuccessStatusCode) return NotFound();

        var user = JsonSerializer.Deserialize<EditInfoViewModel>(
            await response.Content.ReadAsStringAsync(),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
        );

        return View(user);
    }


    [HttpPost]
    public async Task<IActionResult> EditInfo(EditInfoViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        AddJwt();

        var json = JsonSerializer.Serialize(model);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _httpClient.PutAsync($"{ApiBaseUrl}/edit-info/{model.Id}", content);

        if (response.IsSuccessStatusCode)
            return RedirectToAction("Index");

        var errorMessage = await response.Content.ReadAsStringAsync();

        if (errorMessage.Contains("usuario"))
            ModelState.AddModelError("Username", errorMessage);
        else if (errorMessage.Contains("correo"))
            ModelState.AddModelError("Email", errorMessage);
        else if (errorMessage.Contains("rol"))
            ModelState.AddModelError("RoleId", errorMessage);
        else
            ModelState.AddModelError("", errorMessage);

        return View(model);
    }




    // GET: /Users/EditPassword/5
    public async Task<IActionResult> EditPassword(int id)
    {
        var model = new EditPasswordViewModel { Id = id };
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> EditPassword(EditPasswordViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        AddJwt();

        var json = JsonSerializer.Serialize(model);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _httpClient.PutAsync($"{ApiBaseUrl}/edit-password/{model.Id}", content);

        if (response.IsSuccessStatusCode)
            return RedirectToAction("Index");

        var error = await response.Content.ReadAsStringAsync();
        ModelState.AddModelError("", error);
        return View(model);
    }



    // GET: /Users/Delete/15
    public async Task<IActionResult> Delete(int id)
    {
        AddJwt();
        var response = await _httpClient.GetAsync($"{ApiBaseUrl}/get-user/{id}");
        if (!response.IsSuccessStatusCode) return NotFound();

        var user = JsonSerializer.Deserialize<UserViewModel>(
            await response.Content.ReadAsStringAsync(),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
        );

        return View(user);
    }

    // POST: /Users/DeleteConfirmed
    [HttpPost]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        AddJwt();
        var response = await _httpClient.DeleteAsync($"{ApiBaseUrl}/UserApi/{id}");

        if (response.IsSuccessStatusCode)
            return RedirectToAction(nameof(Index));

        var error = await response.Content.ReadAsStringAsync();
        ModelState.AddModelError("", error);

        //Para mostrar error en la vista de Delete
        var userResponse = await _httpClient.GetAsync($"{ApiBaseUrl}/get-user/{id}");
        if (userResponse.IsSuccessStatusCode)
        {
            var user = JsonSerializer.Deserialize<UserViewModel>(
                await userResponse.Content.ReadAsStringAsync(),
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );
            return View("Delete", user);
        }

        return RedirectToAction(nameof(Index));
    }


    public IActionResult ChangePassword()
    {
        return View(new ChangePasswordViewModel());
    }

    [HttpPost]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        AddJwt(); // agrega el token al header

        var json = JsonSerializer.Serialize(model);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _httpClient.PutAsync($"{ApiBaseUrl}/users/change-password", content);

        if (response.IsSuccessStatusCode)
            return RedirectToAction("Index");

        var error = await response.Content.ReadAsStringAsync();
        ModelState.AddModelError("", error);
        return View(model);
    }


}

