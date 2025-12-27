using OrderService.Data;
using Shared.Contracts;
using Shared.Contracts.Events;
using System.Text.Json;

namespace OrderService.OrderHandling
{
    public class OrderUpdatedHandler : IOrderEventHandler
    {
        private readonly IOrderRepository _repository;

        public OrderUpdatedHandler(IOrderRepository i_Repository)
        {
            _repository = i_Repository;
        }

        public EventType EventType => EventType.OrderUpdated;

        public void Handle(JsonElement i_Payload)
        {
            var update = i_Payload.Deserialize<UpdateOrderRequest>();

            if (update == null)
            {
                Console.WriteLine("OrderUpdated payload is null");
                return;
            }

            var existing = _repository.GetById(update.OrderId);

            if (existing == null)
            {
                Console.WriteLine($"Order {update.OrderId} not found");
                return;
            }

            existing.Order.Status = update.OrderStatus;

            Console.WriteLine($"Order {update.OrderId} updated!");
        }
    }
}
