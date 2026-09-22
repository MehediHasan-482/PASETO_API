using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using Org.BouncyCastle.Asn1.Ocsp;
using PasetoMinimalApi.Services;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace PasetoMinimalApi.Auth
{

    class PasetoAuthHandler : AuthenticationHandler<PasetoAuthOptions>
    {
        private readonly IPasetoService _paseto;

        public PasetoAuthHandler(
            IOptionsMonitor<PasetoAuthOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            IPasetoService paseto)
            : base(options, logger, encoder)
        {
            _paseto = paseto;
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.TryGetValue("Authorization", out var header))
                return Task.FromResult(AuthenticateResult.Fail("Missing Authorization header"));

            var token = header.ToString();

            if (!token.StartsWith("Bearer "))
                return Task.FromResult(AuthenticateResult.Fail("Invalid format"));

            token = token["Bearer ".Length..].Trim();

            var result = _paseto.ValidateToken(token);

            if (result is null || !result.IsValid)
                return Task.FromResult(AuthenticateResult.Fail("Invalid or expired token"));

            //  SAFE extraction
            var payload = result.Paseto.Payload;

            var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, payload["sub"]?.ToString() ?? ""),
            new Claim(ClaimTypes.Name, payload["username"]?.ToString() ?? ""),
            new Claim(ClaimTypes.Role, payload["role"]?.ToString() ?? "")
        };

            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);

            return Task.FromResult(
                AuthenticateResult.Success(new AuthenticationTicket(principal, Scheme.Name))
            );
        }
    }
}
