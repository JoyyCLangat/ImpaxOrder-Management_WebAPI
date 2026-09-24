using Microsoft.EntityFrameworkCore;
using OrderManagement.Domain.Entities;

namespace OrderManagement.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<LineItem> LineItems => Set<LineItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Customer>(e =>
        {
            e.HasKey(c => c.Id);
            e.HasIndex(c => c.Email).IsUnique();
            e.Property(c => c.Name).IsRequired().HasMaxLength(200);
            e.Property(c => c.Email).IsRequired().HasMaxLength(200);
        });

        modelBuilder.Entity<Order>(e =>
        {
            e.HasKey(o => o.Id);
            e.HasIndex(o => o.IdempotencyKey).IsUnique();
            e.Property(o => o.IdempotencyKey).IsRequired().HasMaxLength(100);
            e.HasOne(o => o.Customer)
                .WithMany(c => c.Orders)
                .HasForeignKey(o => o.CustomerId);
        });

        modelBuilder.Entity<LineItem>(e =>
        {
            e.HasKey(li => li.Id);
            e.Property(li => li.ProductName).IsRequired().HasMaxLength(200);
            e.Property(li => li.UnitPrice).HasColumnType("decimal(18,2)");
            e.HasOne(li => li.Order)
                .WithMany(o => o.LineItems)
                .HasForeignKey(li => li.OrderId);
        });

        SeedData(modelBuilder);
    }

    private static void SeedData(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Customer>().HasData(
            new Customer { Id = 1, Name = "Wanjiku Kamau", Email = "wanjiku@example.com" },
            new Customer { Id = 2, Name = "Brian Ochieng", Email = "brian@example.com" },
            new Customer { Id = 3, Name = "Amina Mwangi", Email = "amina@example.com" }
        );

        modelBuilder.Entity<Order>().HasData(
            new { Id = 1, IdempotencyKey = "ORD-001", OrderDate = new DateTime(2024, 1, 15, 0, 0, 0, DateTimeKind.Utc), Status = Domain.Enums.OrderStatus.Delivered, CustomerId = 1 },
            new { Id = 2, IdempotencyKey = "ORD-002", OrderDate = new DateTime(2024, 2, 20, 0, 0, 0, DateTimeKind.Utc), Status = Domain.Enums.OrderStatus.Processing, CustomerId = 1 },
            new { Id = 3, IdempotencyKey = "ORD-003", OrderDate = new DateTime(2024, 3, 10, 0, 0, 0, DateTimeKind.Utc), Status = Domain.Enums.OrderStatus.Pending, CustomerId = 2 }
        );

        modelBuilder.Entity<LineItem>().HasData(
            new LineItem { Id = 1, ProductName = "Laptop", Quantity = 1, UnitPrice = 85000.00m, OrderId = 1 },
            new LineItem { Id = 2, ProductName = "Mouse", Quantity = 2, UnitPrice = 2500.00m, OrderId = 1 },
            new LineItem { Id = 3, ProductName = "Keyboard", Quantity = 1, UnitPrice = 4500.00m, OrderId = 2 },
            new LineItem { Id = 4, ProductName = "Monitor", Quantity = 1, UnitPrice = 35000.00m, OrderId = 3 }
        );
    }
}
