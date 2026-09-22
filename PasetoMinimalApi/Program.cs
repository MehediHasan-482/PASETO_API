using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using PasetoMinimalApi.Auth;
using PasetoMinimalApi.Data;
using PasetoMinimalApi.Dto;
using PasetoMinimalApi.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddSingleton<IPasetoService, PasetoService>();

builder.Services
    .AddAuthentication("Paseto")
    .AddScheme<PasetoAuthOptions, PasetoAuthHandler>("Paseto", _ => { });

builder.Services.AddAuthorization();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.UseAuthentication();
app.UseAuthorization();

app.MapPost("/api/auth/login", async (LoginRequest req, IPasetoService paseto, AppDbContext db) =>
{
    var user = await db.Users
        .FirstOrDefaultAsync(u => u.Username.ToLower() == req.Username.ToLower());

    if (user == null || user.PasswordHash != req.Password)
        return Results.Unauthorized();

    var token = paseto.GenerateToken(user.UserId, req.Username, user.Role);

    return Results.Ok(new
    {
        token,
        tokenType = "Bearer",
        expiresInMinutes = 60
    });
})
.AllowAnonymous();
app.MapGet("/api/users/quickfill", async (AppDbContext db) =>
{
    var users = await db.Users
        .Select(u => new { u.Username, u.Role, })
        .OrderBy(u => u.Username)
        .ToListAsync();

    return Results.Ok(users);
})
.AllowAnonymous();

app.MapGet("/api/users/me", (ClaimsPrincipal user) =>
{
    return Results.Ok(new
    {
        userId = user.FindFirstValue(ClaimTypes.NameIdentifier),
        username = user.FindFirstValue(ClaimTypes.Name),
        role = user.FindFirstValue(ClaimTypes.Role)
    });
})
.RequireAuthorization();

app.MapGet("/api/users/admin-only", (ClaimsPrincipal user) =>
{
    if (user.FindFirstValue(ClaimTypes.Role) != "Admin")
        return Results.Forbid();

    return Results.Ok(new { message = "Welcome Admin" });
})
.RequireAuthorization();

app.MapGet("/health", () => Results.Ok(new { status = "Healthy" }))
.AllowAnonymous();

app.MapPost("/api/users/create-profile", (CreateProfileRequest req, ClaimsPrincipal user) =>
{
    var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
    var username = user.FindFirstValue(ClaimTypes.Name);

    if (string.IsNullOrWhiteSpace(req.FullName) || string.IsNullOrWhiteSpace(req.Email))
        return Results.BadRequest(new ApiResponse { Success = false, Message = "Invalid input data" });

    var newProfile = new
    {
        Id = Guid.NewGuid().ToString(),
        UserId = userId,
        FullName = req.FullName,
        Email = req.Email,
        CreatedBy = username
    };

    return Results.Ok(new ApiResponse { Success = true, Message = "Profile created successfully", Data = newProfile });
})
.RequireAuthorization();

app.Run();