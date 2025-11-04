using IlpRepoBackend.Domain.Enum;

namespace IlpRepoBackend.Domain.Persistence
{
    public interface IAuthService
    {
        string GenerateToken(int userId, string email, UserRole role);
        string GeneratePasswordSetupToken(int userId, string email);
        string HashPassword(string password);
        bool VerifyPasswordHash(string password, string passwordHash);
        bool ValidateToken(string token);
        bool ValidatePasswordSetupToken(string token, out int userId);
    }
}