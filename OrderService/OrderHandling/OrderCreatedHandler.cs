using OrderService.Data;
using OrderService.ShippingCost;
using Shared.Contracts.Events;
using Shared.Contracts.Orders;
using System.Text.Json;

namespace OrderService.OrderHandling
{
    public class OrderCreatedHandler : IOrderEventHandler
    {
        private readonly IOrderRepository _repository;
        public OrderCreatedHandler(IOrderRepository i_Repository)
        {
            _repository = i_Repository;
        }

        public EventType EventType => EventType.OrderCreated;

        public void Handle(JsonElement i_Payload)
        {
            var order = i_Payload.Deserialize<Order>();

            if (order == null)
            {
                Console.WriteLine("OrderCreated payload is null");
                return;
            }

            decimal shippingCost = ShippingCostService.CalculateShippingCost(order);

            OrderDetails orderDetails = new OrderDetails(order, shippingCost);

            _repository.Add(orderDetails);

            Console.WriteLine("Order created!");
        }
    }
}
