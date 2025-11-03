using IlpRepoBackend.Domain.Entities;
using IlpRepoBackend.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using IlpRepoBackend.Domain.Persistence;
using System.Threading.Tasks;

namespace IlpRepoBackend.Infrastructure.Repositories
{
    public class UserRepository : GenericRepository<User>, IUserRepository
    {
        private readonly AppDbContext _context;

        public UserRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }

        public Task<bool> EmailExistsAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return Task.FromResult(false);
                
            var normalizedEmail = email.Trim().ToLowerInvariant();
            return _context.Users.AnyAsync(u => u.Email.ToLower() == normalizedEmail);
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return null;

            var normalizedEmail = email.Trim().ToLowerInvariant();
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Email.ToLower() == normalizedEmail);
        }

        public async Task<User?> GetByUsernameAsync(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                return null;

            var normalizedUsername = username.Trim();
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Username == normalizedUsername);
        }

        public Task<bool> UsernameExistsAsync(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                return Task.FromResult(false);
                
            var normalizedUsername = username.Trim();
            return _context.Users.AnyAsync(u => u.Username == normalizedUsername);
        }
    }
}
