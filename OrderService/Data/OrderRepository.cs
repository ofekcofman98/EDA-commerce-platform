using Shared.Contracts;
using System.Collections.Concurrent;

namespace OrderService.Data
{
    public class OrderRepository : IOrderRepository
    {
        private readonly ConcurrentDictionary<string, OrderDetails> _orderDetailsMap =
                    new ConcurrentDictionary<string, OrderDetails>();
        public void Add(OrderDetails i_OrderDetails)
        {
            _orderDetailsMap.TryAdd(i_OrderDetails.Order.OrderId, i_OrderDetails);
        }
        
        public OrderDetails? GetById(string i_OrderId)
        {
            Console.WriteLine($"Starting GetById({i_OrderId})");

            if (_orderDetailsMap.TryGetValue(i_OrderId, out OrderDetails? o_OrderDetails))
            {
                return o_OrderDetails;
            }

            return null;
        }

        public void PrintAllOrderIds() 
        {
            Console.WriteLine("All orders:");
            if (_orderDetailsMap != null)
            {
                foreach (var od in _orderDetailsMap.Values)
                {
                    Console.WriteLine($"orderId: {od.Order.OrderId}");
                }
            }

        }
    }
}
