using Microsoft.EntityFrameworkCore;
using BanhCanhCaLoc.Domain.Entities;

namespace BanhCanhCaLoc.Infrastructure.Persistence
{
    public class BanhCanhCaLocDbContext : DbContext
    {
        public BanhCanhCaLocDbContext(DbContextOptions<BanhCanhCaLocDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Area> Areas { get; set; }
        public DbSet<Table> Tables { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<MenuItem> MenuItems { get; set; }
        public DbSet<Ingredient> Ingredients { get; set; }
        public DbSet<RecipeItem> RecipeItems { get; set; }
        public DbSet<Shift> Shifts { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<StockEntry> StockEntries { get; set; }
        public DbSet<WasteRecord> WasteRecords { get; set; }
        public DbSet<StockTake> StockTakes { get; set; }
        public DbSet<ActivityLog> ActivityLogs { get; set; }
        public DbSet<ShopInfo> ShopInfos { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User - Username Unique
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();

            // RecipeItem Composite Primary Key
            modelBuilder.Entity<RecipeItem>()
                .HasKey(r => new { r.MenuItemId, r.IngredientId });

            modelBuilder.Entity<RecipeItem>()
                .HasOne(r => r.MenuItem)
                .WithMany()
                .HasForeignKey(r => r.MenuItemId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<RecipeItem>()
                .HasOne(r => r.Ingredient)
                .WithMany()
                .HasForeignKey(r => r.IngredientId)
                .OnDelete(DeleteBehavior.Restrict);

            // Table relationships
            modelBuilder.Entity<Table>()
                .HasOne(t => t.Area)
                .WithMany()
                .HasForeignKey(t => t.AreaId)
                .OnDelete(DeleteBehavior.Restrict);

            // MenuItem relationships
            modelBuilder.Entity<MenuItem>()
                .HasOne(m => m.Category)
                .WithMany()
                .HasForeignKey(m => m.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // Shift relationships
            modelBuilder.Entity<Shift>()
                .HasOne(s => s.User)
                .WithMany()
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Order relationships
            modelBuilder.Entity<Order>()
                .HasOne(o => o.Table)
                .WithMany()
                .HasForeignKey(o => o.TableId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Order>()
                .HasOne(o => o.Shift)
                .WithMany()
                .HasForeignKey(o => o.ShiftId)
                .OnDelete(DeleteBehavior.Restrict);

            // OrderItem relationships
            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Order)
                .WithMany(o => o.Items)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.MenuItem)
                .WithMany()
                .HasForeignKey(oi => oi.MenuItemId)
                .OnDelete(DeleteBehavior.Restrict);

            // Payment relationships
            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Order)
                .WithOne()
                .HasForeignKey<Payment>(p => p.OrderId)
                .OnDelete(DeleteBehavior.Restrict);

            // Stock records relationships
            modelBuilder.Entity<StockEntry>()
                .HasOne(se => se.Ingredient)
                .WithMany()
                .HasForeignKey(se => se.IngredientId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<WasteRecord>()
                .HasOne(w => w.Ingredient)
                .WithMany()
                .HasForeignKey(w => w.IngredientId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<StockTake>()
                .HasOne(st => st.Ingredient)
                .WithMany()
                .HasForeignKey(st => st.IngredientId)
                .OnDelete(DeleteBehavior.Restrict);

            // ActivityLog relationships
            modelBuilder.Entity<ActivityLog>()
                .HasOne(a => a.User)
                .WithMany()
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            // Configure decimal properties precision to avoid EF warnings
            modelBuilder.Entity<MenuItem>()
                .Property(m => m.Price)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Payment>()
                .Property(p => p.TotalAmount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Shift>()
                .Property(s => s.TotalRevenue)
                .HasPrecision(18, 2);

            modelBuilder.Entity<StockEntry>()
                .Property(se => se.UnitPrice)
                .HasPrecision(18, 2);
        }
    }
}
