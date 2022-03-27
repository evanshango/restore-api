using System.Reflection;
using API.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class StoreContext : IdentityDbContext<User> {
    public DbSet<Product> Products { get; set; }
    public DbSet<Basket> Baskets { get; set; }

    public StoreContext(DbContextOptions options) : base(options) {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        modelBuilder.Entity<Product>().ToTable("products");
        modelBuilder.Entity<Basket>().ToTable("baskets");
        modelBuilder.Entity<BasketItem>().ToTable("basket_items");
        modelBuilder.Entity<User>().ToTable("users");
        modelBuilder.Entity<IdentityRole>().ToTable("roles").HasData(
            new IdentityRole { Name = "Member", NormalizedName = "MEMBER" },
            new IdentityRole { Name = "Admin", NormalizedName = "ADMIN" }
        );
    }
}