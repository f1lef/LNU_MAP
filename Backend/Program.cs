using Backend.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Додаємо підтримку контролерів API.
builder.Services.AddControllers();

// Додаємо документацію API.
builder.Services.AddOpenApi();

// Читаємо рядок підключення з налаштувань.
var connectionString = builder.Configuration
    .GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "Не знайдено підключення DefaultConnection.");

// Реєструємо контекст для роботи з PostgreSQL.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

// Створюємо застосунок.
var app = builder.Build();

// Документація доступна під час розробки.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Перенаправляємо HTTP-запити на HTTPS.
app.UseHttpsRedirection();

app.UseAuthorization();

// Підключаємо маршрути контролерів.
app.MapControllers();

// Якщо маєш власний app.MapPost("/api/login", ...),
// залиш його тут, перед app.Run().

// Запускаємо API.
app.Run();