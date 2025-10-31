using IlpRepoBackend.Domain.Entities;
using IlpRepoBackend.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using IlpRepoBackend.Domain.Persistence;

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
            return _context.Users.AnyAsync(u => u.Email == email);
        }

        public async Task<User?> GetByUsernameAsync(string username)
        {
            if (string.IsNullOrEmpty(username))
                return null;

            return await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
        }

        public Task GetUserByEmail(string email)
        {
            throw new NotImplementedException();
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            if (string.IsNullOrEmpty(email))
                return null;

            return await _context.Users
                .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
        }


        public Task LoginUser(string email, string password)
        {
            throw new NotImplementedException();
        }

        public Task<bool> UsernameExistsAsync(string username)
        {
            return _context.Users.AnyAsync(u => u.Username == username);
        }
    }
}
