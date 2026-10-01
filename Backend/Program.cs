using Backend.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")
    ));

// Додаємо підтримку контролерів.
// Додаємо підтримку контролерів.
builder.Services.AddControllers();

var app = builder.Build();

// Підключаємо адреси, описані в контролерах.
app.MapControllers();

app.Run();