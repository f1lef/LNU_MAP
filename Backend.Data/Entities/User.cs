namespace Backend.Data.Entities
{
    // Модель користувача нашого застосунку.
    public class User
    {
        // Унікальний ідентифікатор користувача.
        // Guid — ідентифікатор вигляду:
        public Guid Id { get; set; }

        // Електронна пошта для входу в обліковий запис.
        public string Email { get; set; } = string.Empty;

        // Хеш пароля. Зберігаємо результат хешування,
        // а не сам пароль, який ввела людина.
        public string PasswordHash { get; set; } = string.Empty;

        // Група, яку вибрав користувач, наприклад "ПМК-13".
        // За нею визначатимемо, який розклад показувати.
        public string Group { get; set; } = string.Empty;
    }
}