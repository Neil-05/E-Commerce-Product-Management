using IdentityService.Entities;

namespace IdentityService.Repositories
{

    public interface IUserRepository
    {
        Task<User?> GetByEmailAsync(string email);
        Task AddUserAsync(User user);
        Task<bool> UserExistsAsync(string email);
        Task UpdateUserAsync(User user);
        
    }
}