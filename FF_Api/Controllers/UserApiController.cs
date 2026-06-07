using FF_Business;
using FF_ModelsDB.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FF_Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class UserApiController : ControllerBase
    {
        private readonly IUserBusiness _userBusiness;

        // CONCTRUCTOR
        public UserApiController(IUserBusiness userBusiness)
        {
            _userBusiness = userBusiness;
        }

        // LLAMADOS
        
        // POST:
        [HttpPost(Name = "CreateUser")]
        public Task<bool> Create(User user)
        {
            var creatingUser = _userBusiness.CreateUserAsync(user);
            return creatingUser;
        }
    }
}
