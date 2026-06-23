using backend.Data;
using backend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(connectionString));

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
    });
builder.Services.AddAuthorization();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Lets choose your labubu",
        Version = "v1",
        Description = "Whicn labubu are you getting today"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Введіть JWT-токен, отриманий з ендпоінта /auth/login."
    });

    options.AddSecurityRequirement(doc => new OpenApiSecurityRequirement
    {
        { new OpenApiSecuritySchemeReference("Bearer", doc), new List<string>() }
    });
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();


app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", () => "MVP Back-End: CRUD-ендпоінти працюють!");

// AUTH
app.MapPost("/auth/register", async (RegisterDto dto, AppDbContext db) =>
{
    if (await db.Users.AnyAsync(u => u.Email == dto.Email))
        return Results.Conflict("Користувач з таким email вже існує.");

    var user = new User
    {
        Name = dto.Name,
        Email = dto.Email,
        PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
        Role = "user"
    };
    db.Users.Add(user);
    await db.SaveChangesAsync();
    return Results.Created($"/users/{user.Id}",
        new { user.Id, user.Name, user.Email, user.Role });
}).WithTags("Auth");

app.MapPost("/auth/login", async (LoginDto dto, AppDbContext db, IConfiguration config) =>
{
    var user = await db.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
    if (user is null || string.IsNullOrEmpty(user.PasswordHash))
        return Results.Unauthorized();

    if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
        return Results.Unauthorized();

    var token = CreateToken(user, config);
    return Results.Ok(new { access_token = token, token_type = "Bearer" });
}).WithTags("Auth");

app.MapGet("/auth/me", (ClaimsPrincipal principal) =>
{
    return Results.Ok(new
    {
        Id = principal.FindFirstValue(ClaimTypes.NameIdentifier),
        Email = principal.FindFirstValue(ClaimTypes.Email),
        Role = principal.FindFirstValue(ClaimTypes.Role)
    });
})
.RequireAuthorization()
.WithTags("Auth");

// USERS
app.MapGet("/users", async (AppDbContext db) =>
    await db.Users.ToListAsync()).WithTags("Users");

app.MapGet("/users/{id}", async (int id, AppDbContext db) =>
    await db.Users.FindAsync(id) is User user
        ? Results.Ok(user)
        : Results.NotFound()).WithTags("Users");

app.MapPost("/users", async (User user, AppDbContext db) =>
{
    db.Users.Add(user);
    await db.SaveChangesAsync();
    return Results.Created($"/users/{user.Id}", user);
}).WithTags("Users");


app.MapPut("/users/{id}", async (int id, UpdateUserDto input, AppDbContext db) =>
{
    var user = await db.Users.FindAsync(id);
    if (user is null) return Results.NotFound();

    user.Name = input.Name;
    user.Email = input.Email;
    await db.SaveChangesAsync();
    return Results.NoContent();
}).WithTags("Users");


// FIGURES
app.MapGet("/figures", async (AppDbContext db) =>
    await db.Figures.ToListAsync())
    .WithTags("Figures");

// BOX
const int BOX_PRICE = 100;

app.MapPost("/box/open", async (ClaimsPrincipal principal, AppDbContext db) =>
{
    var userId = int.Parse(principal.FindFirstValue(ClaimTypes.NameIdentifier)!);
    var user = await db.Users.FindAsync(userId);
    if (user is null) return Results.NotFound();

    if (user.Balance < BOX_PRICE)
        return Results.BadRequest("Недостатньо коштів на балансі.");

    var figures = await db.Figures.ToListAsync();
    if (figures.Count == 0)
        return Results.Problem("Каталог фігурок порожній.");

    var totalWeight = figures.Sum(f => f.DropWeight);
    var roll = Random.Shared.Next(0, totalWeight);

    LabubuFigure? wonFigure = null;
    var cumulative = 0;
    foreach (var figure in figures)
    {
        cumulative += figure.DropWeight;
        if (roll < cumulative)
        {
            wonFigure = figure;
            break;
        }
    }
    wonFigure ??= figures[^1];

    user.Balance -= BOX_PRICE;

    var item = new InventoryItem
    {
        UserId = userId,
        FigureId = wonFigure.Id,
        ObtainedAt = DateTime.UtcNow
    };
    db.InventoryItems.Add(item);
    await db.SaveChangesAsync();

    return Results.Ok(new { Figure = wonFigure, BalanceAfter = user.Balance });
})
.RequireAuthorization()
.WithTags("Box");

app.MapGet("/inventory", async (ClaimsPrincipal principal, AppDbContext db) =>
{
    var userId = int.Parse(principal.FindFirstValue(ClaimTypes.NameIdentifier)!);
    var items = await db.InventoryItems
        .Where(i => i.UserId == userId)
        .Include(i => i.Figure)
        .OrderByDescending(i => i.ObtainedAt)
        .ToListAsync();
    return Results.Ok(items);
})
.RequireAuthorization()
.WithTags("Box");


app.Run();

static string CreateToken(User user, IConfiguration config)
{
    var claims = new[]
    {
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new Claim(ClaimTypes.Email, user.Email),
        new Claim(ClaimTypes.Role, user.Role)
    };
    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]!));
    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
    var token = new JwtSecurityToken(
        issuer: config["Jwt:Issuer"],
        audience: config["Jwt:Audience"],
        claims: claims,
        expires: DateTime.UtcNow.AddHours(2),
        signingCredentials: creds);
    return new JwtSecurityTokenHandler().WriteToken(token);
}

record RegisterDto(string Name, string Email, string Password);
record LoginDto(string Email, string Password);
record UpdateUserDto(string Name, string Email);