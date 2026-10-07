using Backend.Data.Entities;
using Backend.Data.Configurations;
using Microsoft.EntityFrameworkCore;

namespace Backend.Data
{
    // Контекст для роботи з базою даних.
    public class AppDbContext : DbContext
    {
        // Отримує налаштування підключення від API.
        public AppDbContext(
            DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        // Набір користувачів.
        // Кожен User має Id, Email, PasswordHash і Group.
        public DbSet<User> Users => Set<User>();

        // Налаштовує моделі та правила їх збереження.
        protected override void OnModelCreating(
            ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Підключає правила з UserConfigurations.cs.
            modelBuilder.ApplyConfiguration(
                new UserConfigurations());
        }
    }
}