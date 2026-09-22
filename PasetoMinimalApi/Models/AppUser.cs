namespace PasetoMinimalApi.Models;

public class AppUser
{
    public int Id { get; set; }
    public string UserId { get; set; } = "";
    public string Username { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    public string Role { get; set; } = "User";
    public DateTime CreatedAt { get; set; }
}