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
    Task<User> FindByIdAsync(int id);
    Task<List<User>> GetAllAsync();
    Task<bool> DeleteAsync(int id);
}

public class UserRepository(FF_DbContext context) : RepositoryBase<User>(context), IUserRepository
{

    public async Task<List<User>> GetAllAsync()
    {
        return await DbContext.Users.ToListAsync();
    }

    //Los find
    public async Task<User?> FindByIdAsync(int id)
    {
        return await DbContext.Users.FirstOrDefaultAsync(u => u.Id == id);
    }

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

    //Crear
    /*public async Task<bool> CreateAsync(User entity)
    {
        DbContext.Users.Add(entity);
        return await DbContext.SaveChangesAsync() > 0;
    }*/

    public async Task<bool> CreateAsync(User user)
    {
        DbContext.Users.Add(user);
        await DbContext.SaveChangesAsync();
        return true;
    }


    //Actualizar
    public async Task<bool> UpdateAsync(User user)
    {
        DbContext.Users.Update(user);
        await DbContext.SaveChangesAsync();
        return true;
    }


    //Eliminar

    public async Task<bool> DeleteAsync(int id)
    {
        var user = await DbContext.Users.FindAsync(id);
        if (user == null) return false;

        DbContext.Users.Remove(user);
        await DbContext.SaveChangesAsync();
        return true;
    }


}