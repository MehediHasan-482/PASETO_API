using Paseto;

namespace PasetoMinimalApi.Services
{
    public interface IPasetoService
    {
        string GenerateToken(string userId, string username, string role, int expiryMinutes = 60);
        PasetoTokenValidationResult? ValidateToken(string token);
    }
}
