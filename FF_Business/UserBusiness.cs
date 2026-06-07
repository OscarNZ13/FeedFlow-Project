using FF_ModelsDB.Models;
using FF_Repositories;

namespace FF_Business;

public interface IUserBusiness
{
    Task<bool> CreateUserAsync(User user);
}

public class UserBusiness(IUserRepository userRepository) : IUserBusiness
{
    private readonly IUserRepository _userRepository = userRepository;
    
    // IMPLEMENTACION DE METODOS DE REPOSITORY
    
    // METODO CREAR USUARIO:
    public Task<bool> CreateUserAsync(User user)
    {
        var newUser = _userRepository.CreateAsync(user);
        return newUser;
    }
}