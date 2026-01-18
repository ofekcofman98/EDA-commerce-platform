using OrderService.BackgroundServices;
using OrderService.Data;
using Shared.Contracts;
using Shared.Contracts.Events;
using Shared.Contracts.Orders;
using System.Text.Json;

namespace OrderService.OrderHandling
{
    public class OrderUpdatedHandler : IOrderEventHandler
    {
        private readonly IOrderRepository _repository;
        private readonly ILogger<OrderUpdatedHandler> _logger;

        public OrderUpdatedHandler(IOrderRepository i_Repository, ILogger<OrderUpdatedHandler> logger)
        {
            _logger = logger;
            _repository = i_Repository;
        }
        

        public EventType EventType => EventType.OrderUpdated;

        public void Handle(JsonElement i_Payload)
        {
            var update = i_Payload.Deserialize<UpdateOrderRequest>();

            if (update == null)
            {
                _logger.LogError("OrderUpdated payload is null");
                return;
            }
            

            var existing = _repository.GetById(update.OrderId);
            
            if (existing == null)
            {
                _logger.LogWarning("Order {OrderId} not found", update.OrderId);
                return;
            }

            if(!Enum.TryParse<OrderStatus>(update.Status, ignoreCase: true, out var status))
            {
                _logger.LogWarning(
                    "Invalid order status received in OrderUpdated event. OrderId: {OrderId}, Status: {Status}",
                    update.OrderId,
                    update.Status);

                return;
            }

            existing.Order.Status = status;

            _logger.LogInformation(
                "Order {OrderId} updated to status {Status}",
                update.OrderId,
                update.Status);
        }
    }
}
