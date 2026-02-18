using CartService.Data;
using CartService.Generators;
using CartService.Interfaces;
using CartService.Producer;
using CartService.Services;
using CartService.Validator;
using Moq;
using Shared.Contracts;
using Shared.Contracts.Events;
using Shared.Contracts.Orders;
using Xunit;

namespace CartService.Tests
{
    public class OrderServiceTests
    {
        private readonly Mock<IOrderRepository> _mockRepository;
        private readonly Mock<IEventProducer> _mockProducer;
        private readonly Mock<IValidationFactory> _mockValidationFactory;
        private readonly Mock<IOrderGenerator> _mockOrderGenerator;
        private readonly Mock<IItemGenerator> _mockItemGenerator;
        private readonly OrderService _orderService;

        public OrderServiceTests()
        {
            _mockRepository = new Mock<IOrderRepository>();
            _mockProducer = new Mock<IEventProducer>();
            _mockValidationFactory = new Mock<IValidationFactory>();
            _mockOrderGenerator = new Mock<IOrderGenerator>();
            _mockItemGenerator = new Mock<IItemGenerator>();

            _orderService = new OrderService(
                _mockOrderGenerator.Object,
                _mockItemGenerator.Object,
                _mockRepository.Object,
                _mockProducer.Object,
                _mockValidationFactory.Object
            );
        }

        [Fact]
        public async Task CreateNewOrder_ShouldReturnSuccess_WhenRequestIsValid()
        {
            // Arrange
            var request = new CreateOrderRequest
            {
                orderId = "ORDER-123",
                numOfItems = 2
            };

            var items = new List<Item>
            {
                new Item { itemId = "ITEM-1", price = 10.0m, quantity = 2 },
                new Item { itemId = "ITEM-2", price = 20.0m, quantity = 1 }
            };

            var order = new Order
            {
                OrderId = "ORDER-123",
                CustomerId = "CUSTOMER-1",
                Currency = "USD",
                Items = items,
                TotalAmount = 40.0m,
                Status = OrderStatus.Pending
            };

            // Setup mock validators
            var mockRequestValidator = new Mock<IValidator<CreateOrderRequest>>();
            mockRequestValidator
                .Setup(v => v.Handle(It.IsAny<CreateOrderRequest>()))
                .Returns(ValidationResult.Success());

            var mockOrderValidator = new Mock<IValidator<Order>>();
            mockOrderValidator
                .Setup(v => v.Handle(It.IsAny<Order>()))
                .Returns(ValidationResult.Success());

            var mockItemValidator = new Mock<IValidator<Item>>();
            mockItemValidator
                .Setup(v => v.Handle(It.IsAny<Item>()))
                .Returns(ValidationResult.Success());

            _mockValidationFactory
                .Setup(f => f.GetRequestChain())
                .Returns(mockRequestValidator.Object);

            _mockValidationFactory
                .Setup(f => f.GetOrderChain())
                .Returns(mockOrderValidator.Object);

            _mockValidationFactory
                .Setup(f => f.GetItemChain())
                .Returns(mockItemValidator.Object);

            _mockItemGenerator
                .Setup(g => g.GenerateItems(It.IsAny<int>()))
                .Returns(items);

            _mockOrderGenerator
                .Setup(g => g.GenerateOrder(It.IsAny<CreateOrderRequest>(), It.IsAny<List<Item>>()))
                .Returns(order);

            _mockProducer
                .Setup(p => p.PublishAsync(It.IsAny<EventEnvelope>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _orderService.CreateNewOrder(request);

            // Assert
            Assert.True(result.IsSuccesful);
            Assert.Null(result.ErrorMessage);
            Assert.NotNull(result.Order);
            Assert.Equal("ORDER-123", result.OrderId);
            Assert.Equal(order, result.Order);

            _mockRepository.Verify(r => r.Add(It.IsAny<Order>()), Times.Once);
            _mockProducer.Verify(
                p => p.PublishAsync(It.Is<EventEnvelope>(
                    e => e.EventType == EventType.OrderCreated && e.OrderId == "ORDER-123"
                )),
                Times.Once
            );
        }

        [Fact]
        public async Task CreateNewOrder_ShouldReturnFailure_WhenValidationFails()
        {
            // Arrange
            var request = new CreateOrderRequest
            {
                orderId = "ORDER-123",
                numOfItems = 0
            };

            var mockRequestValidator = new Mock<IValidator<CreateOrderRequest>>();
            mockRequestValidator
                .Setup(v => v.Handle(It.IsAny<CreateOrderRequest>()))
                .Returns(ValidationResult.Failure("Number of items must be greater than 0"));

            _mockValidationFactory
                .Setup(f => f.GetRequestChain())
                .Returns(mockRequestValidator.Object);

            // Act
            var result = await _orderService.CreateNewOrder(request);

            // Assert
            Assert.False(result.IsSuccesful);
            Assert.NotNull(result.ErrorMessage);
            Assert.Equal("Number of items must be greater than 0", result.ErrorMessage);
            Assert.Null(result.Order);

            _mockRepository.Verify(r => r.Add(It.IsAny<Order>()), Times.Never);
            _mockProducer.Verify(p => p.PublishAsync(It.IsAny<EventEnvelope>()), Times.Never);
        }

        [Fact]
        public async Task UpdateOrderStatus_ShouldReturnNotFound_WhenOrderDoesNotExist()
        {
            // Arrange
            var request = new UpdateOrderRequest
            {
                OrderId = "NON-EXISTENT-ORDER",
                Status = "Shipped"
            };

            _mockRepository
                .Setup(r => r.GetById("NON-EXISTENT-ORDER"))
                .Returns((Order?)null);

            // Act
            var result = await _orderService.UpdateOrderStatus(request);

            // Assert
            Assert.False(result.IsSuccesful);
            Assert.NotNull(result.ErrorMessage);
            Assert.Equal("Order NON-EXISTENT-ORDER not found", result.ErrorMessage);
            Assert.Null(result.Order);

            _mockProducer.Verify(p => p.PublishAsync(It.IsAny<EventEnvelope>()), Times.Never);
        }
    }
}

