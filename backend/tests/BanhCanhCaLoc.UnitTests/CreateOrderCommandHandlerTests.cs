using Moq;
using Xunit;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BanhCanhCaLoc.Application.Features.Orders.Commands.CreateOrder;
using BanhCanhCaLoc.Domain.Repositories;
using BanhCanhCaLoc.Domain.Entities;

namespace BanhCanhCaLoc.UnitTests
{
    public class CreateOrderCommandHandlerTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IOrderRepository> _orderRepoMock;
        private readonly Mock<IGenericRepository<ActivityLog, Guid>> _activityLogRepoMock;
        private readonly CreateOrderCommandHandler _handler;

        public CreateOrderCommandHandlerTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _orderRepoMock = new Mock<IOrderRepository>();
            _activityLogRepoMock = new Mock<IGenericRepository<ActivityLog, Guid>>();

            _unitOfWorkMock.Setup(u => u.Orders).Returns(_orderRepoMock.Object);
            _unitOfWorkMock.Setup(u => u.Repository<ActivityLog, Guid>()).Returns(_activityLogRepoMock.Object);

            _handler = new CreateOrderCommandHandler(_unitOfWorkMock.Object);
        }

        [Fact]
        public async Task Handle_Should_CreateOrderWithItems_WhenCommandIsValid()
        {
            // Arrange
            var tableId = 1;
            var shiftId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var orderNumber = "OD-001";
            var itemsInput = new List<CreateOrderItemInput>
            {
                new CreateOrderItemInput(1, 2, "Không hành"),
                new CreateOrderItemInput(2, 1, "Thêm chả cá")
            };
            var command = new CreateOrderCommand(tableId, shiftId, userId, itemsInput);

            _orderRepoMock.Setup(r => r.GenerateOrderNumberAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(orderNumber);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.Equal(orderNumber, result.Value.OrderNumber);
            Assert.Equal(tableId, result.Value.TableId);
            Assert.Equal(shiftId, result.Value.ShiftId);
            Assert.Equal("confirmed", result.Value.Status);
            
            // Check items are populated in the returned order entity
            Assert.Equal(2, result.Value.Items.Count);
            Assert.Equal(1, result.Value.Items[0].MenuItemId);
            Assert.Equal(2, result.Value.Items[0].Quantity);
            Assert.Equal("Không hành", result.Value.Items[0].Note);
            Assert.Equal(2, result.Value.Items[1].MenuItemId);
            Assert.Equal(1, result.Value.Items[1].Quantity);
            Assert.Equal("Thêm chả cá", result.Value.Items[1].Note);

            // Verify order was added via repo
            _orderRepoMock.Verify(r => r.AddAsync(It.Is<Order>(o => o.OrderNumber == orderNumber), It.IsAny<CancellationToken>()), Times.Once);
            
            // Verify activity log was saved
            _activityLogRepoMock.Verify(r => r.AddAsync(It.Is<ActivityLog>(l => l.UserId == userId && l.Action == "create_order"), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
