using System.Configuration;
using System.Data;
using System.Windows;

namespace PasetoApiTesterProj
{
    public partial class App : Application
    {

        public static string? Token { get; set; }
        public static string BaseUrl { get; set; } = "https://localhost:7000";

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            var login = new LoginWindow();
            login.Show();
        }
    }

}
