using Xunit;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using BanhCanhCaLoc.Infrastructure.Persistence;
using BanhCanhCaLoc.Infrastructure.Repositories;
using BanhCanhCaLoc.Domain.Entities;

namespace BanhCanhCaLoc.IntegrationTests
{
    public class OrderRepositoryIntegrationTests
    {
        private BanhCanhCaLocDbContext CreateDbContext(string databaseName)
        {
            var options = new DbContextOptionsBuilder<BanhCanhCaLocDbContext>()
                .UseInMemoryDatabase(databaseName)
                .Options;

            return new BanhCanhCaLocDbContext(options);
        }

        [Fact]
        public async Task GetWithItemsAndTableAsync_Should_ReturnOrderWithDetailsLoaded()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            using var context = CreateDbContext(dbName);
            
            // Seed data
            var area = new Area { Id = 1, Name = "Trong nhà" };
            var table = new Table { Id = 1, Number = 101, Capacity = 4, AreaId = 1, Area = area };
            var category = new Category { Id = 1, Name = "Bánh canh" };
            var menuItem = new MenuItem { Id = 1, Name = "Bánh canh cá lóc đặc biệt", Price = 45000, CategoryId = 1, Category = category, IsAvailable = true };
            
            context.Areas.Add(area);
            context.Tables.Add(table);
            context.Categories.Add(category);
            context.MenuItems.Add(menuItem);
            await context.SaveChangesAsync();

            var order = new Order
            {
                Id = Guid.NewGuid(),
                TableId = table.Id,
                OrderNumber = "OD-001",
                Status = "confirmed",
                CreatedAt = DateTime.UtcNow
            };

            var orderItem = new OrderItem
            {
                Id = Guid.NewGuid(),
                OrderId = order.Id,
                MenuItemId = menuItem.Id,
                Quantity = 2,
                Note = "Không hành"
            };
            order.Items.Add(orderItem);

            context.Orders.Add(order);
            await context.SaveChangesAsync();

            var repo = new OrderRepository(context);

            // Act
            var resultOrder = await repo.GetWithItemsAndTableAsync(order.Id, CancellationToken.None);

            // Assert
            Assert.NotNull(resultOrder);
            Assert.Equal("OD-001", resultOrder.OrderNumber);
            
            // Check Table is loaded
            Assert.NotNull(resultOrder.Table);
            Assert.Equal(101, resultOrder.Table.Number);

            // Check Items are loaded
            Assert.Single(resultOrder.Items);
            var resultItem = resultOrder.Items[0];
            Assert.Equal(2, resultItem.Quantity);
            Assert.Equal("Không hành", resultItem.Note);

            // Check MenuItem on OrderItem is loaded
            Assert.NotNull(resultItem.MenuItem);
            Assert.Equal("Bánh canh cá lóc đặc biệt", resultItem.MenuItem.Name);
            Assert.Equal(45000, resultItem.MenuItem.Price);
        }
    }
}
