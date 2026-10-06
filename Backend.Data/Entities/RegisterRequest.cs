namespace Backend.Data.Entities;

public class RegisterRequest
{
    // Пошта нового користувача.
    public string Email { get; set; } = string.Empty;

    // Пароль, який прийде від користувача.
    // У БД напряму не зберігається.
    public string Password { get; set; } = string.Empty;

    // Академічна група.
    public string Group { get; set; } = string.Empty;
}