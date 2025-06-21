using TheRealInvision.Models.DbModels;

namespace TheRealInvision.Interfaces
{
    public interface IUserRepository
    {
        Task<IEnumerable<User>> GetAllAsync();
        Task<User?> GetByIdAsync(Guid id);
        Task AddWithHashedPasswordAsync(User user, string plainPassword);
        Task UpdateAsync(User user);
        Task DeleteAsync(Guid id, Guid deletedBy);
        Task<User?> GetByEmailAndPasswordAsync(string email, string password);
    }
}
