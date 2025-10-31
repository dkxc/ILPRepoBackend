using IlpRepoBackend.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IlpRepoBackend.Domain.Persistence
{
    public interface IUserRepository : IGenericRepository<User>
    {
        Task<bool> EmailExistsAsync(string email);
        Task<User> GetByUsernameAsync(string username);
        Task GetUserByEmail(string email);
        Task LoginUser(string email, string password);
        Task<bool> UsernameExistsAsync(string username);
       
        Task<User?> GetUserByEmailAsync(string email);
    }
}
