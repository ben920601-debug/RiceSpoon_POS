using Microsoft.EntityFrameworkCore;
using RiceCookerPos.Models;

namespace RiceCookerPos.Data
{
    public class PosDbContext : DbContext
    {
        public PosDbContext(DbContextOptions<PosDbContext> options) : base(options) { }

        // 定義對應的 MySQL 資料表
        public DbSet<Staff> Staffs { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 設定各資料表的主鍵 (Primary Key)
            modelBuilder.Entity<Staff>().HasKey(s => s.StaffCode);
            modelBuilder.Entity<Product>().HasKey(p => p.Id);
            modelBuilder.Entity<Order>().HasKey(o => o.OrderId);
            
            // 💡 補上這行：設定 OrderItem 的主鍵
            modelBuilder.Entity<OrderItem>().HasKey(oi => oi.Id);
            
            // 處理訂單與明細的一對多關係
            modelBuilder.Entity<Order>()
                .HasMany(o => o.Items)
                .WithOne()
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}