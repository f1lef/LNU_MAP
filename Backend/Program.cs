var builder = WebApplication.CreateBuilder(args);

// Додаємо підтримку контролерів.
// Додаємо підтримку контролерів.
builder.Services.AddControllers();

var app = builder.Build();

// Підключаємо адреси, описані в контролерах.
app.MapControllers();

app.Run();