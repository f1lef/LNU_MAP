namespace Backend.Data.Entities;

public class LoginRequest
{
    // Пошта, яку користувач вводить під час входу.
    public string Email { get; set; } = string.Empty;

    // Пароль, який користувач вводить під час входу.
    // У базу даних цей пароль не записується.
    public string Password { get; set; } = string.Empty;
}