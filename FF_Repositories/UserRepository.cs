using FF_DataDB.Context;
using FF_ModelsDB.Models;
using Microsoft.EntityFrameworkCore;

namespace FF_Repositories;

public interface IUserRepository
{
    Task<bool> UpsertAsync(User entity, bool isUpdating);
    Task<bool> CreateAsync(User entity);
    Task<bool> DeleteAsync(User entity);
    Task<IEnumerable<User>> ReadAsync();
    Task<User> FindAsync(int id);
    Task<bool> UpdateAsync(User entity);
    Task<bool> UpdateManyAsync(IEnumerable<User> entities);
    Task<bool> ExistsAsync(User entity);
    Task<User?> FindByUsernameAsync(string username);
    Task<User?> FindByEmailAsync(string email);
}

public class UserRepository(FF_DbContext context) : RepositoryBase<User>(context), IUserRepository
{
    public async Task<User?> FindByUsernameAsync(string username)
    {
        return await DbContext.Users
            .FirstOrDefaultAsync(u => u.Username == username);
    }
    public async Task<User?> FindByEmailAsync(string email)
    {
        return await DbContext.Users
            .FirstOrDefaultAsync(u => u.Email == email);
    }
}