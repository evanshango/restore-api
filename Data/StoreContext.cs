using System.Reflection;
using API.Entities;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class StoreContext : DbContext {
    public DbSet<Product> Products { get; set; }
    public DbSet<Basket> Baskets { get; set; }

    public StoreContext(DbContextOptions<StoreContext> options) : base(options) {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        modelBuilder.Entity<Product>().ToTable("products");
        modelBuilder.Entity<Basket>().ToTable("baskets");
        modelBuilder.Entity<BasketItem>().ToTable("basket_items");
    }
}