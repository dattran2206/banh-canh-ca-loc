using BanhCanhCaLoc.Api.Models;
using System;
using System.Linq;

namespace BanhCanhCaLoc.Api.Data
{
    public static class DbInitializer
    {
        public static void Initialize(BanhCanhCaLocDbContext context)
        {
            // Ensure schema is created
            context.Database.EnsureCreated();

            // Seed Users
            if (!context.Users.Any())
            {
                var adminHash = BCrypt.Net.BCrypt.HashPassword("admin123");
                var waiterHash = BCrypt.Net.BCrypt.HashPassword("waiter123");
                var cashierHash = BCrypt.Net.BCrypt.HashPassword("cashier123");
                var kitchenHash = BCrypt.Net.BCrypt.HashPassword("kitchen123");

                context.Users.AddRange(
                    new User { Id = Guid.NewGuid(), Username = "admin", PasswordHash = adminHash, Role = "admin", FullName = "Chủ Quán", IsActive = true },
                    new User { Id = Guid.NewGuid(), Username = "waiter01", PasswordHash = waiterHash, Role = "waiter", FullName = "Bồi Bàn 1", IsActive = true },
                    new User { Id = Guid.NewGuid(), Username = "cashier01", PasswordHash = cashierHash, Role = "cashier", FullName = "Thu Ngân 1", IsActive = true },
                    new User { Id = Guid.NewGuid(), Username = "kitchen01", PasswordHash = kitchenHash, Role = "kitchen", FullName = "Bếp 1", IsActive = true }
                );
                context.SaveChanges();
            }

            // Seed Areas
            if (!context.Areas.Any())
            {
                context.Areas.AddRange(
                    new Area { Name = "Trong nhà" },
                    new Area { Name = "Ngoài trời" }
                );
                context.SaveChanges();
            }

            // Seed Tables
            if (!context.Tables.Any())
            {
                var indoor = context.Areas.FirstOrDefault(a => a.Name == "Trong nhà");
                var outdoor = context.Areas.FirstOrDefault(a => a.Name == "Ngoài trời");

                if (indoor != null && outdoor != null)
                {
                    context.Tables.AddRange(
                        new Table { Number = 1, AreaId = indoor.Id, Capacity = 4 },
                        new Table { Number = 2, AreaId = indoor.Id, Capacity = 4 },
                        new Table { Number = 3, AreaId = indoor.Id, Capacity = 4 },
                        new Table { Number = 4, AreaId = indoor.Id, Capacity = 6 },
                        new Table { Number = 5, AreaId = indoor.Id, Capacity = 6 },
                        new Table { Number = 6, AreaId = outdoor.Id, Capacity = 4 },
                        new Table { Number = 7, AreaId = outdoor.Id, Capacity = 4 },
                        new Table { Number = 8, AreaId = outdoor.Id, Capacity = 6 },
                        new Table { Number = 9, AreaId = outdoor.Id, Capacity = 2 },
                        new Table { Number = 10, AreaId = outdoor.Id, Capacity = 2 }
                    );
                    context.SaveChanges();
                }
            }

            // Seed Categories
            if (!context.Categories.Any())
            {
                context.Categories.AddRange(
                    new Category { Name = "Món chính" },
                    new Category { Name = "Topping" },
                    new Category { Name = "Nước uống" },
                    new Category { Name = "Khác" }
                );
                context.SaveChanges();
            }

            // Seed Menu Items
            if (!context.MenuItems.Any())
            {
                var monChinh = context.Categories.FirstOrDefault(c => c.Name == "Món chính");
                var topping = context.Categories.FirstOrDefault(c => c.Name == "Topping");
                var nuocUong = context.Categories.FirstOrDefault(c => c.Name == "Nước uống");

                if (monChinh != null && topping != null && nuocUong != null)
                {
                    context.MenuItems.AddRange(
                        new MenuItem { Name = "Bánh canh cá lóc thường", CategoryId = monChinh.Id, Price = 45000, Description = "Tô bánh canh cá lóc size thường", IsAvailable = true },
                        new MenuItem { Name = "Bánh canh cá lóc lớn", CategoryId = monChinh.Id, Price = 55000, Description = "Tô bánh canh cá lóc size lớn", IsAvailable = true },
                        new MenuItem { Name = "Bánh canh cá lóc đặc biệt", CategoryId = monChinh.Id, Price = 65000, Description = "Tô đặc biệt nhiều cá, thêm trứng cút", IsAvailable = true },
                        new MenuItem { Name = "Thêm cá lóc", CategoryId = topping.Id, Price = 20000, Description = "Thêm phần cá lóc", IsAvailable = true },
                        new MenuItem { Name = "Thêm trứng cút", CategoryId = topping.Id, Price = 8000, Description = "Thêm 4 trứng cút", IsAvailable = true },
                        new MenuItem { Name = "Thêm bánh canh", CategoryId = topping.Id, Price = 10000, Description = "Thêm phần bánh canh", IsAvailable = true },
                        new MenuItem { Name = "Trà đá", CategoryId = nuocUong.Id, Price = 5000, Description = "Trà đá miễn phí refill", IsAvailable = true },
                        new MenuItem { Name = "Nước ngọt", CategoryId = nuocUong.Id, Price = 15000, Description = "Coca, Pepsi, 7UP", IsAvailable = true },
                        new MenuItem { Name = "Nước suối", CategoryId = nuocUong.Id, Price = 10000, Description = "Nước suối chai 500ml", IsAvailable = true }
                    );
                    context.SaveChanges();
                }
            }

            // Seed Ingredients
            if (!context.Ingredients.Any())
            {
                context.Ingredients.AddRange(
                    new Ingredient { Name = "Cá lóc tươi", Unit = "kg", CurrentStock = 10, MinThreshold = 2 },
                    new Ingredient { Name = "Bánh canh sợi", Unit = "kg", CurrentStock = 15, MinThreshold = 3 },
                    new Ingredient { Name = "Nước lèo", Unit = "lít", CurrentStock = 20, MinThreshold = 5 },
                    new Ingredient { Name = "Hành lá", Unit = "kg", CurrentStock = 1, MinThreshold = 0.2 },
                    new Ingredient { Name = "Rau ăn kèm", Unit = "kg", CurrentStock = 2, MinThreshold = 0.5 },
                    new Ingredient { Name = "Trứng cút", Unit = "cái", CurrentStock = 100, MinThreshold = 20 },
                    new Ingredient { Name = "Gia vị tổng hợp", Unit = "kg", CurrentStock = 2, MinThreshold = 0.3 }
                );
                context.SaveChanges();
            }

            // Seed Recipes
            if (!context.RecipeItems.Any())
            {
                var m1 = context.MenuItems.FirstOrDefault(m => m.Name == "Bánh canh cá lóc thường");
                var m2 = context.MenuItems.FirstOrDefault(m => m.Name == "Bánh canh cá lóc lớn");
                var m3 = context.MenuItems.FirstOrDefault(m => m.Name == "Bánh canh cá lóc đặc biệt");
                var m4 = context.MenuItems.FirstOrDefault(m => m.Name == "Thêm cá lóc");
                var m5 = context.MenuItems.FirstOrDefault(m => m.Name == "Thêm trứng cút");
                var m6 = context.MenuItems.FirstOrDefault(m => m.Name == "Thêm bánh canh");

                var ing1 = context.Ingredients.FirstOrDefault(i => i.Name == "Cá lóc tươi");
                var ing2 = context.Ingredients.FirstOrDefault(i => i.Name == "Bánh canh sợi");
                var ing3 = context.Ingredients.FirstOrDefault(i => i.Name == "Nước lèo");
                var ing4 = context.Ingredients.FirstOrDefault(i => i.Name == "Hành lá");
                var ing5 = context.Ingredients.FirstOrDefault(i => i.Name == "Rau ăn kèm");
                var ing6 = context.Ingredients.FirstOrDefault(i => i.Name == "Trứng cút");

                if (ing1 != null && ing2 != null && ing3 != null && ing4 != null && ing5 != null)
                {
                    // Bánh canh thường
                    if (m1 != null)
                    {
                        context.RecipeItems.AddRange(
                            new RecipeItem { MenuItemId = m1.Id, IngredientId = ing1.Id, Quantity = 0.15, YieldPercent = 0.7 },
                            new RecipeItem { MenuItemId = m1.Id, IngredientId = ing2.Id, Quantity = 0.1, YieldPercent = 1.0 },
                            new RecipeItem { MenuItemId = m1.Id, IngredientId = ing3.Id, Quantity = 0.5, YieldPercent = 1.0 },
                            new RecipeItem { MenuItemId = m1.Id, IngredientId = ing4.Id, Quantity = 0.01, YieldPercent = 1.0 },
                            new RecipeItem { MenuItemId = m1.Id, IngredientId = ing5.Id, Quantity = 0.05, YieldPercent = 1.0 }
                        );
                    }
                    // Bánh canh lớn
                    if (m2 != null)
                    {
                        context.RecipeItems.AddRange(
                            new RecipeItem { MenuItemId = m2.Id, IngredientId = ing1.Id, Quantity = 0.2, YieldPercent = 0.7 },
                            new RecipeItem { MenuItemId = m2.Id, IngredientId = ing2.Id, Quantity = 0.13, YieldPercent = 1.0 },
                            new RecipeItem { MenuItemId = m2.Id, IngredientId = ing3.Id, Quantity = 0.65, YieldPercent = 1.0 },
                            new RecipeItem { MenuItemId = m2.Id, IngredientId = ing4.Id, Quantity = 0.012, YieldPercent = 1.0 },
                            new RecipeItem { MenuItemId = m2.Id, IngredientId = ing5.Id, Quantity = 0.06, YieldPercent = 1.0 }
                        );
                    }
                    // Bánh canh đặc biệt
                    if (m3 != null && ing6 != null)
                    {
                        context.RecipeItems.AddRange(
                            new RecipeItem { MenuItemId = m3.Id, IngredientId = ing1.Id, Quantity = 0.25, YieldPercent = 0.7 },
                            new RecipeItem { MenuItemId = m3.Id, IngredientId = ing2.Id, Quantity = 0.15, YieldPercent = 1.0 },
                            new RecipeItem { MenuItemId = m3.Id, IngredientId = ing3.Id, Quantity = 0.7, YieldPercent = 1.0 },
                            new RecipeItem { MenuItemId = m3.Id, IngredientId = ing4.Id, Quantity = 0.015, YieldPercent = 1.0 },
                            new RecipeItem { MenuItemId = m3.Id, IngredientId = ing5.Id, Quantity = 0.07, YieldPercent = 1.0 },
                            new RecipeItem { MenuItemId = m3.Id, IngredientId = ing6.Id, Quantity = 4, YieldPercent = 1.0 }
                        );
                    }
                    // Topping
                    if (m4 != null)
                    {
                        context.RecipeItems.Add(new RecipeItem { MenuItemId = m4.Id, IngredientId = ing1.Id, Quantity = 0.15, YieldPercent = 0.7 });
                    }
                    if (m5 != null && ing6 != null)
                    {
                        context.RecipeItems.Add(new RecipeItem { MenuItemId = m5.Id, IngredientId = ing6.Id, Quantity = 4, YieldPercent = 1.0 });
                    }
                    if (m6 != null)
                    {
                        context.RecipeItems.Add(new RecipeItem { MenuItemId = m6.Id, IngredientId = ing2.Id, Quantity = 0.1, YieldPercent = 1.0 });
                    }
                    context.SaveChanges();
                }
            }

            // Seed ShopInfo
            if (!context.ShopInfos.Any())
            {
                context.ShopInfos.Add(new ShopInfo
                {
                    Name = "Quán Bánh Canh Cá Lóc",
                    Address = "123 Đường Lê Lợi, Q.1, TP.HCM",
                    Phone = "0901 234 567"
                });
                context.SaveChanges();
            }
        }
    }
}
