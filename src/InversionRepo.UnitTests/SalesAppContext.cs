
using InversionRepo.UnitTests.Entities;
using Microsoft.EntityFrameworkCore;

namespace InversionRepo.UnitTests
{
    public class SalesAppContext : DbContext
    {
        public SalesAppContext(DbContextOptions<SalesAppContext> options) : base(options)
        {
            Database.EnsureCreated(); // this should be used only with in-memory database for unit testing
        }

        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductDiscount> ProductDiscounts { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            DataSeeding.Seed(modelBuilder);
        }
    }
}
