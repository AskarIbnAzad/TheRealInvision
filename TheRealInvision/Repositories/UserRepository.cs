using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TheRealInvision.Interfaces;
using TheRealInvision.Models;
using TheRealInvision.Models.DbModels;

namespace TheRealInvision.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;
        private readonly PasswordHasher<User> _passwordHasher = new PasswordHasher<User>();

        public UserRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<User>> GetAllAsync()
        {
            return await _context.Users.Where(p=>p.Status==EntityStatus.Active).ToListAsync();
        }

        public async Task<User?> GetByIdAsync(Guid id)
        {
            return await _context.Users.FindAsync(id);
        }

        public async Task AddWithHashedPasswordAsync(User user, string plainPassword)
        {
            user.Password = _passwordHasher.HashPassword(user, plainPassword);
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(User user)
        {
            user.Password = _passwordHasher.HashPassword(user, user.Password);
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id,Guid deletedBy)
        {
            var user = await GetByIdAsync(id);
            if (user != null)
            {
                user.Status = EntityStatus.Deleted;
                user.UpdatedBy = deletedBy;
                user.UpdatedDate = DateTime.Now;
                _context.Users.Update(user);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<User?> GetByEmailAndPasswordAsync(string email, string plainPassword)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email && u.Status==EntityStatus.Active);
            if (user == null) return null;

            var result = _passwordHasher.VerifyHashedPassword(user, user.Password, plainPassword);
            return result == PasswordVerificationResult.Success ? user : null;
        }
    }
}
