using OrderService.Interfaces;
using OrderService.Services;
using Shared.Contracts.Events;
using Shared.Contracts.Orders;
using System.Text.Json;

namespace OrderService.Services
{
    public class OrderCreatedHandler : IOrderEventHandler
    {
        private readonly IOrderRepository _repository;
        private readonly ILogger<OrderCreatedHandler> _logger;

        public OrderCreatedHandler(IOrderRepository i_Repository, ILogger<OrderCreatedHandler> logger)
        {
            _repository = i_Repository;
            _logger = logger;
        }

        public EventType EventType => EventType.OrderCreated;

        public void Handle(JsonElement i_Payload)
        {
            var order = i_Payload.Deserialize<Order>();

            if (order == null)
            {
                _logger.LogError("OrderUpdated payload is null");
                return;
            }

            decimal shippingCost = ShippingCostService.CalculateShippingCost(order);

            OrderDetails orderDetails = new OrderDetails(order, shippingCost);

            _repository.Add(orderDetails);

            _logger.LogInformation(
                "Order {OrderId} create! Status: {Status}, Shipping cost: {shippingCost}",
                orderDetails.Order.OrderId,
                orderDetails.Order.Status,
                orderDetails.shippingCost);
        }
    }
}
