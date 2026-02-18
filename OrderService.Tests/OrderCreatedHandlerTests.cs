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
    public class OrderCreatedHandlerTests
    {
        private readonly Mock<IOrderRepository> _mockRepository;
        private readonly Mock<ILogger<OrderCreatedHandler>> _mockLogger;
        private readonly OrderCreatedHandler _handler;

        public OrderCreatedHandlerTests()
        {
            _mockRepository = new Mock<IOrderRepository>();
            _mockLogger = new Mock<ILogger<OrderCreatedHandler>>();
            _handler = new OrderCreatedHandler(_mockRepository.Object, _mockLogger.Object);
        }

        [Fact]
        public void HandleAsync_ShouldSaveOrder_WhenValidEventReceived()
        {
            // Arrange
            var order = new Order
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

            var jsonPayload = JsonSerializer.SerializeToElement(order);

            // Act
            _handler.Handle(jsonPayload);

            // Assert
            _mockRepository.Verify(
                r => r.Add(It.Is<OrderDetails>(
                    od => od.Order.OrderId == "ORDER-123" 
                    && od.Order.CustomerId == "CUSTOMER-1"
                    && od.Order.TotalAmount == 200.0m
                    && od.shippingCost > 0
                )),
                Times.Once,
                "Repository.Add should be called exactly once with the correct order data"
            );

            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("ORDER-123") && v.ToString()!.Contains("create")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once,
                "Logger should log information about order creation"
            );
        }

        [Fact]
        public void HandleAsync_ShouldNotSave_WhenPayloadIsInvalid()
        {
            // Arrange - Create malformed JSON that will result in null order after deserialization
            var invalidJson = "null";
            var jsonElement = JsonSerializer.Deserialize<JsonElement>(invalidJson);

            // Act
            _handler.Handle(jsonElement);

            // Assert
            _mockRepository.Verify(
                r => r.Add(It.IsAny<OrderDetails>()),
                Times.Never,
                "Repository.Add should not be called when payload is invalid"
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
        public void HandleAsync_ShouldCalculateShippingCost_WhenValidEventReceived()
        {
            // Arrange
            var order = new Order
            {
                OrderId = "ORDER-456",
                CustomerId = "CUSTOMER-2",
                OrderDate = DateTime.UtcNow,
                Items = new List<Item>
                {
                    new Item { itemId = "ITEM-1", price = 50.0m, quantity = 3 }
                },
                TotalAmount = 150.0m,
                Currency = "EUR",
                Status = OrderStatus.Pending
            };

            var jsonPayload = JsonSerializer.SerializeToElement(order);

            // Act
            _handler.Handle(jsonPayload);

            // Assert
            _mockRepository.Verify(
                r => r.Add(It.Is<OrderDetails>(od => od.shippingCost > 0)),
                Times.Once,
                "Shipping cost should be calculated and greater than 0"
            );
        }
    }
}

