using Backend.Data;
using Backend.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Backend.Controllers;


// [ApiController] говорить ASP.NET Core,
// що цей клас є API-контролером.
[ApiController]

// Усі адреси в цьому контролері починаються з:
// /api/auth
[Route("api/auth")]
public class AuthController : ControllerBase
{
    // Через AppDbContext ми працюємо з PostgreSQL.
    private readonly AppDbContext _context;

    // PasswordHasher потрібен для перевірки пароля
    // з хешем, який зберігається в базі даних.
    private readonly IPasswordHasher<User> _passwordHasher;

    // IConfiguration дозволяє читати налаштування,
    // наприклад JWT-ключ із User Secrets.
    private readonly IConfiguration _configuration;


    // Конструктор.
    // ASP.NET Core сам передасть сюди всі потрібні сервіси
    // через Dependency Injection.
    public AuthController(
        AppDbContext context,
        IPasswordHasher<User> passwordHasher,
        IConfiguration configuration)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _configuration = configuration;
    }


    // Цей метод буде викликатися через:
    // POST /api/auth/login
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        // Перевіряємо, чи всі поля заповнені.
        if (string.IsNullOrWhiteSpace(request.Email) ||
            string.IsNullOrWhiteSpace(request.Password) ||
            string.IsNullOrWhiteSpace(request.Group))
        {
            return BadRequest(new
            {
                message = "Email, password і group є обов'язковими."
            });
        }

        // Нормалізуємо email.
        string email = request.Email
            .Trim()
            .ToLowerInvariant();

        // Перевіряємо, чи такий email уже є в базі.
        bool emailExists = await _context.Users
            .AnyAsync(u => u.Email == email);

        if (emailExists)
        {
            return BadRequest(new
            {
                message = "Користувач з такою поштою вже існує."
            });
        }

        // Створюємо нового користувача.
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            Group = request.Group.Trim(),
            PasswordHash = string.Empty
        };

        // Хешуємо пароль.
        user.PasswordHash =
            _passwordHasher.HashPassword(
                user,
                request.Password);

        // Додаємо користувача в PostgreSQL.
        _context.Users.Add(user);

        await _context.SaveChangesAsync();

        // Повертаємо 201 Created.
        return StatusCode(201, new
        {
            message = "Користувача успішно зареєстровано.",
            userId = user.Id,
            email = user.Email,
            group = user.Group
        });
    }
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        // -------------------------------------------------
        // 1. ПЕРЕВІРКА ОБОВ'ЯЗКОВИХ ПОЛІВ
        // -------------------------------------------------

        // Якщо email або password порожні,
        // запит некоректний -> повертаємо 400 Bad Request.
        if (string.IsNullOrWhiteSpace(request.Email) ||
            string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new
            {
                message = "Email і password є обов'язковими."
            });
        }


        // Забираємо пробіли перед і після email.
        // Також переводимо email у нижній регістр.
        //
        // Наприклад:
        // " Test@Test.com " -> "test@test.com"
        string email = request.Email
            .Trim()
            .ToLower();


        // -------------------------------------------------
        // 2. ПОШУК КОРИСТУВАЧА В POSTGRESQL
        // -------------------------------------------------

        // _context.Users - це таблиця Users.
        //
        // FirstOrDefaultAsync шукає першого користувача,
        // email якого збігається з введеним.
        //
        // Якщо такого користувача немає,
        // результат буде null.
        User? user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == email);


        // Якщо користувача не знайдено,
        // повертаємо 401 Unauthorized.
        //
        // ВАЖЛИВО:
        // не пишемо "такого email не існує",
        // бо це підказувало б сторонній людині,
        // які акаунти є в системі.
        if (user == null)
        {
            return Unauthorized(new
            {
                message = "Неправильний email або пароль."
            });
        }


        // -------------------------------------------------
        // 3. ПЕРЕВІРКА ПАРОЛЯ
        // -------------------------------------------------

        // У PostgreSQL пароль НЕ зберігається як звичайний текст.
        //
        // Там лежить PasswordHash.
        //
        // VerifyHashedPassword отримує:
        // 1. користувача;
        // 2. хеш із бази;
        // 3. пароль, який користувач зараз ввів.
        //
        // Після цього PasswordHasher визначає,
        // чи відповідає пароль хешу.
        PasswordVerificationResult passwordResult =
            _passwordHasher.VerifyHashedPassword(
                user,
                user.PasswordHash,
                request.Password);


        // Якщо пароль неправильний,
        // повертаємо той самий 401,
        // що й для неправильного email.
        if (passwordResult == PasswordVerificationResult.Failed)
        {
            return Unauthorized(new
            {
                message = "Неправильний email або пароль."
            });
        }


        // -------------------------------------------------
        // 4. ОТРИМУЄМО JWT-КЛЮЧ
        // -------------------------------------------------

        // Ключ беремо з конфігурації.
        //
        // Ми зберігаємо його через User Secrets,
        // тому він не потрапляє в GitHub.
        string? jwtKey = _configuration["Jwt:Key"];


        // Якщо ключ не налаштований,
        // це вже проблема сервера, тому повертаємо 500.
        if (string.IsNullOrWhiteSpace(jwtKey))
        {
            return Problem(
                "JWT key is not configured.",
                statusCode: 500);
        }


        // -------------------------------------------------
        // 5. ЧАС ЗАВЕРШЕННЯ JWT
        // -------------------------------------------------

        // Наш JWT буде працювати 1 годину.
        //
        // Використовуємо UTC, щоб час не залежав
        // від часового поясу комп'ютера.
        DateTime expiresAt = DateTime.UtcNow.AddHours(1);


        // -------------------------------------------------
        // 6. ДАНІ, ЯКІ ЗАПИСУЄМО В JWT
        // -------------------------------------------------

        // Claims - це інформація всередині токена.
        var claims = new List<Claim>
        {
            // ID користувача.
            new Claim(
                "userId",
                user.Id.ToString()),

            // Email користувача.
            new Claim(
                ClaimTypes.Email,
                user.Email)
        };


        // -------------------------------------------------
        // 7. ПІДПИС JWT
        // -------------------------------------------------

        // Перетворюємо наш секретний ключ
        // у формат, який потрібен для JWT.
        var securityKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtKey));


        // Вказуємо алгоритм цифрового підпису.
        //
        // HmacSha256 означає,
        // що токен підписується алгоритмом SHA-256.
        var credentials = new SigningCredentials(
            securityKey,
            SecurityAlgorithms.HmacSha256);


        // -------------------------------------------------
        // 8. СТВОРЮЄМО JWT
        // -------------------------------------------------

        var token = new JwtSecurityToken(
            claims: claims,

            // Час, після якого токен стає недійсним.
            expires: expiresAt,

            // Цифровий підпис.
            signingCredentials: credentials
        );


        // JwtSecurityToken поки є C#-об'єктом.
        //
        // WriteToken перетворює його у звичайний рядок:
        //
        // eyJhbGciOiJIUzI1NiIs...
        string tokenString =
            new JwtSecurityTokenHandler()
                .WriteToken(token);


        // -------------------------------------------------
        // 9. УСПІШНИЙ ВХІД
        // -------------------------------------------------

        // Якщо ми дійшли сюди:
        //
        // email правильний
        // +
        // password правильний
        //
        // Тому повертаємо HTTP 200 OK.
        return Ok(new
        {
            token = tokenString,

            // ID користувача.
            userId = user.Id,

            // Email користувача.
            email = user.Email,

            // Коли JWT перестане працювати.
            expiresAt = expiresAt
        });
    }
}