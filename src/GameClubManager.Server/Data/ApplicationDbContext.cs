using GameClubManager.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace GameClubManager.Server.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Computer> Computers { get; set; }
    public DbSet<Booking> Bookings { get; set; }
    public DbSet<Payment> Payments { get; set; }
    public DbSet<ComputerTelemetry> ComputerTelemetries { get; set; }
    public DbSet<ProcessInfo> ProcessInfos { get; set; }
    public DbSet<SystemAlert> SystemAlerts { get; set; }
    public DbSet<Penalty> Penalties { get; set; }
    public DbSet<GamePreference> GamePreferences { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Username)
            .IsUnique();

        modelBuilder.Entity<User>()
            .Property(u => u.Balance)
            .HasPrecision(18, 2);

        modelBuilder.Entity<User>()
            .HasMany(u => u.Penalties)
            .WithOne()
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<User>()
            .HasMany(u => u.GamePreferences)
            .WithOne()
            .HasForeignKey(gp => gp.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Computer>()
            .Property(c => c.PricePerHour)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Booking>()
            .Property(b => b.TotalPrice)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Payment>()
            .Property(p => p.Amount)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Penalty>()
            .Property(p => p.Amount)
            .HasPrecision(18, 2);

        modelBuilder.Entity<ComputerTelemetry>()
            .HasMany(ct => ct.RunningProcesses)
            .WithOne()
            .HasForeignKey(pi => pi.ComputerTelemetryId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ComputerTelemetry>()
            .HasMany(ct => ct.Alerts)
            .WithOne()
            .HasForeignKey(sa => sa.Id)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ProcessInfo>()
            .HasKey(pi => pi.Id);

        modelBuilder.Entity<SystemAlert>()
            .HasKey(sa => sa.Id);
    }

    public async Task InitializeDatabaseAsync()
    {
        // Создаем базу данных, если она не существует
        await Database.EnsureCreatedAsync();
        
        // Проверяем наличие пользователей
        if (!Users.Any())
        {
            // Создаем администратора
            var admin = new User
            {
                Username = "admin",
                Email = "admin@example.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin"),
                Role = UserRole.Admin,
                Balance = 1000,
                RemainingTime = TimeSpan.FromHours(10)
            };

            // Создаем тестового пользователя
            var testUser = new User
            {
                Username = "test",
                Email = "v@mail.ru",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("test"),
                Role = UserRole.User,
                Balance = 500,
                RemainingTime = TimeSpan.FromHours(5)
            };

            Users.Add(admin);
            Users.Add(testUser);
            await SaveChangesAsync();
        }
    }
} 