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
    public DbSet<FoodItem> FoodItems { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<AdminNotification> AdminNotifications { get; set; }
    public DbSet<Game> Games { get; set; }

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

        modelBuilder.Entity<Order>()
            .HasOne(o => o.User)
            .WithMany(u => u.Orders)
            .HasForeignKey(o => o.UserId);

        modelBuilder.Entity<OrderItem>()
            .HasOne(oi => oi.Order)
            .WithMany(o => o.Items)
            .HasForeignKey(oi => oi.OrderId);

        modelBuilder.Entity<OrderItem>()
            .HasOne(oi => oi.FoodItem)
            .WithMany()
            .HasForeignKey(oi => oi.FoodItemId);

        // Конфигурация для модели AdminNotification
        modelBuilder.Entity<AdminNotification>()
            .Property(a => a.Title)
            .HasMaxLength(100)
            .IsRequired();
        
        modelBuilder.Entity<AdminNotification>()
            .Property(a => a.Message)
            .HasMaxLength(500)
            .IsRequired();
        
        modelBuilder.Entity<AdminNotification>()
            .Property(a => a.Type)
            .HasMaxLength(50)
            .IsRequired();
        
        // Связи для таблицы AdminNotifications
        modelBuilder.Entity<AdminNotification>()
            .HasOne<Computer>()
            .WithMany()
            .HasForeignKey(a => a.ComputerId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);
        
        modelBuilder.Entity<AdminNotification>()
            .HasOne<User>()
            .WithMany()
            .HasForeignKey(a => a.UserId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);
        
        // Конфигурация для модели FoodItem
        modelBuilder.Entity<FoodItem>()
            .Property(f => f.Price)
            .HasPrecision(18, 2);
        
        // Конфигурация для модели Order
        modelBuilder.Entity<Order>()
            .Property(o => o.TotalAmount)
            .HasPrecision(18, 2);
        
        // Конфигурация для модели OrderItem
        modelBuilder.Entity<OrderItem>()
            .Property(oi => oi.Price)
            .HasPrecision(18, 2);
        
        // Конфигурация для модели Game
        modelBuilder.Entity<Game>()
            .Property(g => g.Name)
            .HasMaxLength(100)
            .IsRequired();
            
        modelBuilder.Entity<Game>()
            .Property(g => g.Description)
            .HasMaxLength(500);
            
        modelBuilder.Entity<Game>()
            .Property(g => g.Developer)
            .HasMaxLength(50);
            
        modelBuilder.Entity<Game>()
            .Property(g => g.Publisher)
            .HasMaxLength(50);
            
        // Связь между GamePreference и Game
        modelBuilder.Entity<GamePreference>()
            .HasOne<Game>()
            .WithMany()
            .HasForeignKey(gp => gp.GameId)
            .OnDelete(DeleteBehavior.Restrict);
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
        
        // Проверяем наличие продуктов питания
        if (!FoodItems.Any())
        {
            var foodItems = new List<FoodItem>
            {
                new FoodItem
                {
                    Name = "Пицца Пепперони",
                    Description = "Классическая пицца с колбасой пепперони, сыром и томатным соусом",
                    Price = 400,
                    IsAvailable = true
                },
                new FoodItem
                {
                    Name = "Кола",
                    Description = "Газированный напиток, 0.5л",
                    Price = 120,
                    IsAvailable = true
                },
                new FoodItem
                {
                    Name = "Чипсы Lays",
                    Description = "Картофельные чипсы с солью, 80г",
                    Price = 150,
                    IsAvailable = true
                },
                new FoodItem
                {
                    Name = "Энергетический напиток Monster",
                    Description = "Энергетический напиток, 0.5л",
                    Price = 180,
                    IsAvailable = true
                },
                new FoodItem
                {
                    Name = "Бургер",
                    Description = "Сочный бургер с говяжьей котлетой, сыром и овощами",
                    Price = 350,
                    IsAvailable = true
                },
                new FoodItem
                {
                    Name = "Шоколадный батончик Snickers",
                    Description = "Батончик с карамелью, арахисом и нугой, 50г",
                    Price = 90,
                    IsAvailable = true
                }
            };
            
            FoodItems.AddRange(foodItems);
            await SaveChangesAsync();
        }
    }
} 