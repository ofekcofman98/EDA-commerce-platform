using Microsoft.Extensions.Logging;
using Moq;
using OrderService.Interfaces;
using OrderService.Services;
using Shared.Contracts;
using Shared.Contracts.Orders;
using System.Text.Json;
using Xunit;

namespace OrderService.Tests
{
    public class OrderUpdatedHandlerTests
    {
        private readonly Mock<IOrderRepository> _mockRepository;
        private readonly Mock<ILogger<OrderUpdatedHandler>> _mockLogger;
        private readonly OrderUpdatedHandler _handler;

        public OrderUpdatedHandlerTests()
        {
            _mockRepository = new Mock<IOrderRepository>();
            _mockLogger = new Mock<ILogger<OrderUpdatedHandler>>();
            _handler = new OrderUpdatedHandler(_mockRepository.Object, _mockLogger.Object);
        }

        [Fact]
        public void Handle_ShouldUpdateOrderStatus_WhenValidEventReceived()
        {
            // Arrange
            var existingOrder = new Order
            {
                OrderId = "ORDER-123",
                CustomerId = "CUSTOMER-1",
                OrderDate = DateTime.UtcNow,
                Items = new List<Item>
                {
                    new Item { itemId = "ITEM-1", price = 100.0m, quantity = 2 }
                },
                TotalAmount = 200.0m,
                Currency = "USD",
                Status = OrderStatus.Pending
            };

            var orderDetails = new OrderDetails(existingOrder, 10.0m);

            var updateRequest = new UpdateOrderRequest
            {
                OrderId = "ORDER-123",
                Status = "Shipped"
            };

            var jsonPayload = JsonSerializer.SerializeToElement(updateRequest);

            _mockRepository
                .Setup(r => r.GetById("ORDER-123"))
                .Returns(orderDetails);

            // Act
            _handler.Handle(jsonPayload);

            // Assert
            Assert.Equal(OrderStatus.Shipped, existingOrder.Status);

            _mockRepository.Verify(
                r => r.GetById("ORDER-123"),
                Times.Once,
                "Repository.GetById should be called to retrieve the existing order"
            );

            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("ORDER-123") && v.ToString()!.Contains("updated")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once,
                "Logger should log information about order update"
            );
        }

        [Fact]
        public void Handle_ShouldNotUpdate_WhenOrderDoesNotExist()
        {
            // Arrange
            var updateRequest = new UpdateOrderRequest
            {
                OrderId = "NON-EXISTENT-ORDER",
                Status = "Shipped"
            };

            var jsonPayload = JsonSerializer.SerializeToElement(updateRequest);

            _mockRepository
                .Setup(r => r.GetById("NON-EXISTENT-ORDER"))
                .Returns((OrderDetails?)null);

            // Act
            _handler.Handle(jsonPayload);

            // Assert
            _mockRepository.Verify(
                r => r.GetById("NON-EXISTENT-ORDER"),
                Times.Once
            );

            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("not found")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once,
                "Logger should log a warning when order is not found"
            );
        }

        [Fact]
        public void Handle_ShouldNotUpdate_WhenPayloadIsInvalid()
        {
            // Arrange - Create malformed JSON that will result in null after deserialization
            var invalidJson = "null";
            var jsonElement = JsonSerializer.Deserialize<JsonElement>(invalidJson);

            // Act
            _handler.Handle(jsonElement);

            // Assert
            _mockRepository.Verify(
                r => r.GetById(It.IsAny<string>()),
                Times.Never,
                "Repository.GetById should not be called when payload is invalid"
            );

            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("null")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once,
                "Logger should log an error when payload is invalid"
            );
        }

        [Fact]
        public void Handle_ShouldNotUpdate_WhenStatusIsInvalid()
        {
            // Arrange
            var existingOrder = new Order
            {
                OrderId = "ORDER-789",
                CustomerId = "CUSTOMER-3",
                OrderDate = DateTime.UtcNow,
                Items = new List<Item>
                {
                    new Item { itemId = "ITEM-1", price = 50.0m, quantity = 1 }
                },
                TotalAmount = 50.0m,
                Currency = "USD",
                Status = OrderStatus.Pending
            };

            var orderDetails = new OrderDetails(existingOrder, 5.0m);

            var updateRequest = new UpdateOrderRequest
            {
                OrderId = "ORDER-789",
                Status = "InvalidStatus"
            };

            var jsonPayload = JsonSerializer.SerializeToElement(updateRequest);

            _mockRepository
                .Setup(r => r.GetById("ORDER-789"))
                .Returns(orderDetails);

            // Act
            _handler.Handle(jsonPayload);

            // Assert
            Assert.Equal(OrderStatus.Pending, existingOrder.Status); // Status should remain unchanged

            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Invalid order status")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once,
                "Logger should log a warning when status is invalid"
            );
        }

        [Theory]
        [InlineData("Pending", OrderStatus.Pending)]
        [InlineData("Shipped", OrderStatus.Shipped)]
        [InlineData("Confirmed", OrderStatus.Confirmed)]
        [InlineData("Cancelled", OrderStatus.Cancelled)]
        [InlineData("Paid", OrderStatus.Paid)]
        public void Handle_ShouldUpdateToCorrectStatus_WhenValidStatusProvided(string statusString, OrderStatus expectedStatus)
        {
            // Arrange
            var existingOrder = new Order
            {
                OrderId = "ORDER-TEST",
                CustomerId = "CUSTOMER-TEST",
                OrderDate = DateTime.UtcNow,
                Items = new List<Item>
                {
                    new Item { itemId = "ITEM-1", price = 100.0m, quantity = 1 }
                },
                TotalAmount = 100.0m,
                Currency = "USD",
                Status = OrderStatus.Pending
            };

            var orderDetails = new OrderDetails(existingOrder, 10.0m);

            var updateRequest = new UpdateOrderRequest
            {
                OrderId = "ORDER-TEST",
                Status = statusString
            };

            var jsonPayload = JsonSerializer.SerializeToElement(updateRequest);

            _mockRepository
                .Setup(r => r.GetById("ORDER-TEST"))
                .Returns(orderDetails);

            // Act
            _handler.Handle(jsonPayload);

            // Assert
            Assert.Equal(expectedStatus, existingOrder.Status);
        }
    }
}

