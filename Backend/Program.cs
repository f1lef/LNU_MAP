using Backend.Data;
using Backend.Data.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);


//  CONTROLLERS 

// Підключаємо контролери API.
// Завдяки цьому працюватиме AuthController.
builder.Services.AddControllers();


//  OPENAPI 

builder.Services.AddOpenApi();


//  DATABASE 

// Беремо connection string для PostgreSQL.
// Пароль PostgreSQL зберігається локально через User Secrets,
// а не безпосередньо в коді.
var connectionString = builder.Configuration
    .GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "Не знайдено підключення DefaultConnection.");

// Реєструємо AppDbContext.
// UseNpgsql означає, що Entity Framework Core працює з PostgreSQL.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));


//  PASSWORD HASHING 

// Реєструємо PasswordHasher.
//
// Під час реєстрації він створює PasswordHash.
// Під час входу він перевіряє введений пароль
// проти PasswordHash із PostgreSQL.
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();


//  JWT 

// Беремо секретний ключ для JWT з User Secrets.
// У Git цей ключ не потрапляє.
var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException(
        "Не знайдено Jwt:Key у User Secrets.");

// Налаштовуємо JWT authentication.
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            // Перевіряємо цифровий підпис JWT.
            ValidateIssuerSigningKey = true,

            // Використовуємо наш секретний ключ.
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtKey)),

            // Поки що не використовуємо Issuer.
            ValidateIssuer = false,

            // Поки що не використовуємо Audience.
            ValidateAudience = false,

            // Перевіряємо, чи JWT ще не протермінований.
            ValidateLifetime = true,

            // Після expires токен одразу стає недійсним.
            ClockSkew = TimeSpan.Zero
        };
    });


// Авторизація потрібна для майбутніх endpoint-ів,
// які будуть доступні тільки після входу.
builder.Services.AddAuthorization();


//  APP 

var app = builder.Build();


// OpenAPI працює тільки в Development.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}


// Перенаправляємо HTTP на HTTPS.
app.UseHttpsRedirection();


// Спочатку Authentication:
// система визначає, хто користувач по JWT.
app.UseAuthentication();

// Потім Authorization:
// система перевіряє, чи має користувач доступ.
app.UseAuthorization();


// Підключаємо маршрути контролерів.
// Наприклад:
// POST /api/auth/login
app.MapControllers();


// Запускаємо API.

app.Run();