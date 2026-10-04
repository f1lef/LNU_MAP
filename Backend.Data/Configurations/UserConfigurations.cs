using Backend.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Data.Configurations
{
    // Правила збереження моделі User у базі даних.
    public class UserConfigurations : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            // Id — первинний ключ таблиці користувачів.
            builder.HasKey(u => u.Id);

            // Email не може бути null.
            // Максимальна довжина — 254 символи.
            builder.Property(u => u.Email)
                .IsRequired()
                .HasMaxLength(254);

            // Однакові значення email не можуть повторюватися.
            builder.HasIndex(u => u.Email)
                .IsUnique();

            // Хеш пароля не може бути null.
            // Саме хешування виконується в коді реєстрації.
            builder.Property(u => u.PasswordHash)
                .IsRequired();

            // Назва вибраної групи, наприклад "ПМК-13".
            // Максимальна довжина — 20 символів.
            builder.Property(u => u.Group)
                .HasMaxLength(20);
        }
    }
}