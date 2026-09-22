using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Windows;
using System.Windows.Media;

namespace PasetoApiTesterProj;

public partial class MainWindow : Window
{
    private readonly HttpClient _http;
    private readonly TokenService _tokenService;


    public MainWindow()
    {
        InitializeComponent();

        var handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback =
                HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        };

        _http = new HttpClient(handler);
        _tokenService = new TokenService(_http);
    }


    private async void Window_Loaded(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(AppSession.Token))
        {
            MessageBox.Show("No session found. Please login again.");
            RedirectToLogin();
            return;
        }

        await CallGet("/api/users/me");
    }


    private async void MeBtn_Click(object sender, RoutedEventArgs e)
        => await CallGet("/api/users/me");

    private async void AdminBtn_Click(object sender, RoutedEventArgs e)
        => await CallGet("/api/users/admin-only");

    private async void ManagerBtn_Click(object sender, RoutedEventArgs e)
        => await CallGet("/api/users/manager-or-admin");

    private async void HealthBtn_Click(object sender, RoutedEventArgs e)
        => await CallGet("/health", useAuth: false);

    private async void CreateProfileBtn_Click(object sender, RoutedEventArgs e)
    {
        await CallPost("/api/users/create-profile", new
        {
            fullName = "John Doe",
            email = "john@example.com"
        });
    }


    private async Task CallGet(string path, bool useAuth = true)
    {
        var url = $"{AppSession.BaseUrl}{path}";
        var sw = Stopwatch.StartNew();

        try
        {
            if (useAuth)
                await _tokenService.EnsureValidTokenAsync();

            var request = new HttpRequestMessage(HttpMethod.Get, url);

            if (useAuth)
            {
                request.Headers.Authorization =
                    new AuthenticationHeaderValue("Bearer", AppSession.Token);
            }

            var response = await _http.SendAsync(request);
            sw.Stop();

            var body = await response.Content.ReadAsStringAsync();

            ResponseBox.Text = PrettyJson(body);
        }
        catch (Exception ex)
        {
            ResponseBox.Text = ex.Message;
        }
    }

    private async Task CallPost(string path, object data, bool useAuth = true)
    {
        var url = $"{AppSession.BaseUrl}{path}";
        var sw = Stopwatch.StartNew();

        try
        {
            if (useAuth)
                await _tokenService.EnsureValidTokenAsync();

            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = JsonContent.Create(data)
            };

            if (useAuth)
            {
                request.Headers.Authorization =
                    new AuthenticationHeaderValue("Bearer", AppSession.Token);
            }

            var response = await _http.SendAsync(request);
            sw.Stop();

            var body = await response.Content.ReadAsStringAsync();

            UpdateStatusBar(response.StatusCode, sw.ElapsedMilliseconds);
            ResponseBox.Text = PrettyJson(body);
        }
        catch (Exception ex)
        {
            ResponseBox.Text = ex.Message;
        }
    }





    private void LogoutBtn_Click(object sender, RoutedEventArgs e)
    {
        AppSession.Token = null;
        RedirectToLogin();
    }

    private void RedirectToLogin()
    {
        var login = new LoginWindow();
        login.Show();
        Close();
    }


    private void CopyBtn_Click(object sender, RoutedEventArgs e)
        => Clipboard.SetText(ResponseBox.Text);

    private void ClearBtn_Click(object sender, RoutedEventArgs e)
    {
        ResponseBox.Text = "";
        LastRequestLabel.Text = "—";
        UpdateStatusBar(null, null);
    }

    private void UpdateStatusBar(System.Net.HttpStatusCode? code, long? ms)
    {
        if (code is null)
        {
            StatusCodeLabel.Text = "—";
            ResponseTimeLabel.Text = "—";
            return;
        }

        StatusCodeLabel.Text = $"{(int)code} {code}";
        ResponseTimeLabel.Text = $"{ms} ms";
    }

   
    private static string PrettyJson(string raw)
    {
        try
        {
            var doc = JsonDocument.Parse(raw);
            return JsonSerializer.Serialize(doc, new JsonSerializerOptions { WriteIndented = true });
        }
        catch
        {
            return raw;
        }
    }


}