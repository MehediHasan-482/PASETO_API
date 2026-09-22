using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;

namespace PasetoApiTesterProj;

public partial class LoginWindow : Window
{
    private readonly HttpClient _http;

    public LoginWindow()
    {
        InitializeComponent();

        var handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback =
                HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        };
        _http = new HttpClient(handler);
    }

    private async void Window_Loaded(object sender, RoutedEventArgs e)
    {
        await LoadQuickFillUsersAsync();
    }

    private async Task LoadQuickFillUsersAsync()
    {
        var baseUrl = BaseUrlBox.Text.TrimEnd('/');
        var url = $"{baseUrl}/api/users/quickfill";

        const int maxRetries = 3;
        const int retryDelayMs = 2000;

        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                var response = await _http.GetAsync(url);

                if (!response.IsSuccessStatusCode) return;

                var body = await response.Content.ReadAsStringAsync();
                var users = JsonSerializer.Deserialize<List<QuickFillUser>>(
                    body,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (users == null) return;

                QuickFillPanel.Children.Clear();

                foreach (var user in users)
                {
                    var btn = new Button
                    {
                        Content = $"{user.Username} / {user.Role}",
                        Style = (Style)FindResource("OutlineButton"),
                        Padding = new Thickness(8, 5, 8, 5),
                        FontSize = 11,
                        Margin = new Thickness(0, 0, 6, 6),
                        Tag = user.Username
                    };

                    btn.Click += (s, _) =>
                    {
                        UsernameBox.Text = (string)((Button)s).Tag;
                        PasswordBox.Password = "";
                        PasswordBox.Focus();
                    };

                    QuickFillPanel.Children.Add(btn);
                }

                return;
            }
            catch
            {
                if (attempt < maxRetries)
                    await Task.Delay(retryDelayMs);
            }
        }
    }

    private async void LoginBtn_Click(object sender, RoutedEventArgs e)
    {
        var baseUrl = BaseUrlBox.Text.TrimEnd('/');
        var username = UsernameBox.Text.Trim();
        var password = PasswordBox.Password;

        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            MessageBox.Show("Username এবং Password দিন।");
            return;
        }

        LoginBtn.IsEnabled = false;
        LoginBtn.Content = "Logging in...";

        try
        {
            var url = $"{baseUrl}/api/auth/login";
            var response = await _http.PostAsJsonAsync(url, new { username, password });
            var body = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                MessageBox.Show("Login failed — username বা password ভুল।");
                return;
            }

            var result = JsonSerializer.Deserialize<LoginResponse>(
                body,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (result == null || string.IsNullOrEmpty(result.Token))
            {
                MessageBox.Show("Invalid server response");
                return;
            }

            AppSession.Token = result.Token;
            AppSession.BaseUrl = baseUrl;
            AppSession.TokenExpiry = DateTime.UtcNow.AddMinutes(result.ExpiresInMinutes);

            var main = new MainWindow();
            main.Show();
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error: {ex.Message}");
        }
        finally
        {
            LoginBtn.IsEnabled = true;
            LoginBtn.Content = "Login";
        }
    }
}

public class QuickFillUser
{
    public string Username { get; set; } = "";
    public string Role { get; set; } = "";
}