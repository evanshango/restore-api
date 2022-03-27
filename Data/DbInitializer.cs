using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public static class DbInitializer {
    public static async Task Initialize(StoreContext context, UserManager<User> userManager) {
        if (!userManager.Users.Any()) {
            var user = new User {
                UserName = "bob",
                Email = "bob@test.com"
            };
            await userManager.CreateAsync(user, "P@$$w0rd");
            await userManager.AddToRoleAsync(user, "Member");

            var admin = new User {
                UserName = "admin",
                Email = "admin@test.com"
            };
            await userManager.CreateAsync(admin, "P@$$w0rd");
            await userManager.AddToRolesAsync(admin, new[] { "Admin", "Member" });
        }

        if (await context.Products.AnyAsync()) return;

        var products = new List<Product> {
            new() {
                Name = "Angular Speedster Board 2000",
                Description =
                    "Lorem ipsum dolor sit amet, consectetuer adipiscing elit. Maecenas porttitor congue massa. Fusce posuere, magna sed pulvinar ultricies, purus lectus malesuada libero, sit amet commodo magna eros quis urna.",
                Price = 20000,
                ImageUrl = "/images/products/sb-ang1.png",
                Brand = "Angular",
                Type = "Boards",
                QtyInStock = 100
            },
            new() {
                Name = "Green Angular Board 3000",
                Description = "Nunc viverra imperdiet enim. Fusce est. Vivamus a tellus.",
                Price = 15000,
                ImageUrl = "/images/products/sb-ang2.png",
                Brand = "Angular",
                Type = "Boards",
                QtyInStock = 100
            },
            new() {
                Name = "Core Board Speed Rush 3",
                Description =
                    "Suspendisse dui purus, scelerisque at, vulputate vitae, pretium mattis, nunc. Mauris eget neque at sem venenatis eleifend. Ut nonummy.",
                Price = 18000,
                ImageUrl = "/images/products/sb-core1.png",
                Brand = "NetCore",
                Type = "Boards",
                QtyInStock = 100
            },
            new() {
                Name = "Net Core Super Board",
                Description =
                    "Pellentesque habitant morbi tristique senectus et netus et malesuada fames ac turpis egestas. Proin pharetra nonummy pede. Mauris et orci.",
                Price = 30000,
                ImageUrl = "/images/products/sb-core2.png",
                Brand = "NetCore",
                Type = "Boards",
                QtyInStock = 100
            },
            new() {
                Name = "React Board Super Whizzy Fast",
                Description =
                    "Lorem ipsum dolor sit amet, consectetuer adipiscing elit. Maecenas porttitor congue massa. Fusce posuere, magna sed pulvinar ultricies, purus lectus malesuada libero, sit amet commodo magna eros quis urna.",
                Price = 25000,
                ImageUrl = "/images/products/sb-react1.png",
                Brand = "React",
                Type = "Boards",
                QtyInStock = 100
            },
            new() {
                Name = "Typescript Entry Board",
                Description =
                    "Lorem ipsum dolor sit amet, consectetuer adipiscing elit. Maecenas porttitor congue massa. Fusce posuere, magna sed pulvinar ultricies, purus lectus malesuada libero, sit amet commodo magna eros quis urna.",
                Price = 12000,
                ImageUrl = "/images/products/sb-ts1.png",
                Brand = "TypeScript",
                Type = "Boards",
                QtyInStock = 100
            },
            new() {
                Name = "Core Blue Hat",
                Description =
                    "Fusce posuere, magna sed pulvinar ultricies, purus lectus malesuada libero, sit amet commodo magna eros quis urna.",
                Price = 1000,
                ImageUrl = "/images/products/hat-core1.png",
                Brand = "NetCore",
                Type = "Hats",
                QtyInStock = 100
            },
            new() {
                Name = "Green React Woolen Hat",
                Description =
                    "Fusce posuere, magna sed pulvinar ultricies, purus lectus malesuada libero, sit amet commodo magna eros quis urna.",
                Price = 8000,
                ImageUrl = "/images/products/hat-react1.png",
                Brand = "React",
                Type = "Hats",
                QtyInStock = 100
            },
            new() {
                Name = "Purple React Woolen Hat",
                Description =
                    "Fusce posuere, magna sed pulvinar ultricies, purus lectus malesuada libero, sit amet commodo magna eros quis urna.",
                Price = 1500,
                ImageUrl = "/images/products/hat-react2.png",
                Brand = "React",
                Type = "Hats",
                QtyInStock = 100
            },
            new() {
                Name = "Blue Code Gloves",
                Description =
                    "Fusce posuere, magna sed pulvinar ultricies, purus lectus malesuada libero, sit amet commodo magna eros quis urna.",
                Price = 1800,
                ImageUrl = "/images/products/glove-code1.png",
                Brand = "VS Code",
                Type = "Gloves",
                QtyInStock = 100
            },
            new() {
                Name = "Green Code Gloves",
                Description =
                    "Fusce posuere, magna sed pulvinar ultricies, purus lectus malesuada libero, sit amet commodo magna eros quis urna.",
                Price = 1500,
                ImageUrl = "/images/products/glove-code2.png",
                Brand = "VS Code",
                Type = "Gloves",
                QtyInStock = 100
            },
            new() {
                Name = "Purple React Gloves",
                Description =
                    "Fusce posuere, magna sed pulvinar ultricies, purus lectus malesuada libero, sit amet commodo magna eros quis urna.",
                Price = 1600,
                ImageUrl = "/images/products/glove-react1.png",
                Brand = "React",
                Type = "Gloves",
                QtyInStock = 100
            },
            new() {
                Name = "Green React Gloves",
                Description =
                    "Fusce posuere, magna sed pulvinar ultricies, purus lectus malesuada libero, sit amet commodo magna eros quis urna.",
                Price = 1400,
                ImageUrl = "/images/products/glove-react2.png",
                Brand = "React",
                Type = "Gloves",
                QtyInStock = 100
            },
            new() {
                Name = "Redis Red Boots",
                Description =
                    "Suspendisse dui purus, scelerisque at, vulputate vitae, pretium mattis, nunc. Mauris eget neque at sem venenatis eleifend. Ut nonummy.",
                Price = 25000,
                ImageUrl = "/images/products/boot-redis1.png",
                Brand = "Redis",
                Type = "Boots",
                QtyInStock = 100
            },
            new Product {
                Name = "Core Red Boots",
                Description =
                    "Lorem ipsum dolor sit amet, consectetuer adipiscing elit. Maecenas porttitor congue massa. Fusce posuere, magna sed pulvinar ultricies, purus lectus malesuada libero, sit amet commodo magna eros quis urna.",
                Price = 18999,
                ImageUrl = "/images/products/boot-core2.png",
                Brand = "NetCore",
                Type = "Boots",
                QtyInStock = 100
            },
            new() {
                Name = "Core Purple Boots",
                Description =
                    "Pellentesque habitant morbi tristique senectus et netus et malesuada fames ac turpis egestas. Proin pharetra nonummy pede. Mauris et orci.",
                Price = 19999,
                ImageUrl = "/images/products/boot-core1.png",
                Brand = "NetCore",
                Type = "Boots",
                QtyInStock = 100
            },
            new() {
                Name = "Angular Purple Boots",
                Description = "Aenean nec lorem. In porttitor. Donec laoreet nonummy augue.",
                Price = 15000,
                ImageUrl = "/images/products/boot-ang2.png",
                Brand = "Angular",
                Type = "Boots",
                QtyInStock = 100
            },
            new() {
                Name = "Angular Blue Boots",
                Description =
                    "Suspendisse dui purus, scelerisque at, vulputate vitae, pretium mattis, nunc. Mauris eget neque at sem venenatis eleifend. Ut nonummy.",
                Price = 18000,
                ImageUrl = "/images/products/boot-ang1.png",
                Brand = "Angular",
                Type = "Boots",
                QtyInStock = 100
            }
        };
        foreach (var product in products) {
            await context.Products.AddAsync(product);
        }

        await context.SaveChangesAsync();
    }
}