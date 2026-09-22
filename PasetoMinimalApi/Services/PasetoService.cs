using Paseto;
using Paseto.Builder;
using Paseto.Cryptography.Key;

namespace PasetoMinimalApi.Services
{
    public class PasetoService: IPasetoService
    {
        private readonly PasetoKey _key;

        public PasetoService()
        {
            _key = new PasetoBuilder()
                .Use(ProtocolVersion.V4, Purpose.Local)
                .GenerateSymmetricKey(); // RETURNS PasetoKey (correct type)
        }

        public string GenerateToken(string userId, string username, string role, int expiryMinutes = 60)
        {
            return new PasetoBuilder()
                .Use(ProtocolVersion.V4, Purpose.Local)
                .WithKey(_key)
                .Subject(userId)
                .AddClaim("username", username)
                .AddClaim("role", role)
                .Issuer("MyApi")
                .IssuedAt(DateTime.UtcNow)
                .Expiration(DateTime.UtcNow.AddMinutes(expiryMinutes))
                .Encode();
        }

        public PasetoTokenValidationResult? ValidateToken(string token)
        {
            try
            {
                return new PasetoBuilder()
                    .Use(ProtocolVersion.V4, Purpose.Local)
                    .WithKey(_key)
                    .Decode(token, new PasetoTokenValidationParameters
                    {
                        ValidateLifetime = true,
                        ValidateIssuer = true,
                        ValidIssuer = "MyApi"
                    });
            }
            catch
            {
                return null;
            }
        }
    }
}
