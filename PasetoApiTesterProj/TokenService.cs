using System.Net.Http;
using System.Net.Http.Json;

namespace PasetoApiTesterProj;

public class TokenService
{
    private readonly HttpClient _http;

    public TokenService(HttpClient http)
    {
        _http = http;
    }

    public async Task EnsureValidTokenAsync()
    {
        if (string.IsNullOrWhiteSpace(AppSession.Token))
            throw new Exception("No token found");

        if (AppSession.TokenExpiry > DateTime.UtcNow.AddMinutes(1))
            return;

        await RefreshTokenAsync();
    }

    private async Task RefreshTokenAsync()
    {
        var url = $"{AppSession.BaseUrl}/api/auth/refresh";

        var response = await _http.PostAsJsonAsync(url, new
        {
            refreshToken = AppSession.RefreshToken
        });

        if (!response.IsSuccessStatusCode)
        {
            AppSession.Token = null;
            AppSession.RefreshToken = null;
            throw new Exception("Session expired");
        }

        var result = await response.Content.ReadFromJsonAsync<LoginResponse>();

        AppSession.Token = result!.Token;
        AppSession.TokenExpiry = DateTime.UtcNow.AddSeconds(result.ExpiresInMinutes);
    }
}
