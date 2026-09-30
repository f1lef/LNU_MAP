// Підключаємо інструменти ASP.NET Core для контролерів.
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

// Позначаємо клас як API-контролер.
[ApiController]

// Задаємо адресу: /health.
[Route("health")]
public class HealthController : ControllerBase
{
    // Цей метод обробляє GET-запити на /health.
    [HttpGet]
    public IActionResult Get()
    {
        // Повертаємо HTTP 200 OK та JSON {"status":"ok"}.
        return Ok(new
        {
            status = "ok"
        });
    }
}