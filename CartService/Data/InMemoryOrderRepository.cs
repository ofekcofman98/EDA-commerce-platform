using Shared.Contracts.Orders;
using System.Collections.Concurrent;

namespace CartService.Data
{
    public class InMemoryOrderRepository : IOrderRepository
    {
        private readonly ConcurrentDictionary<string, Order> r_Orders = new ConcurrentDictionary<string, Order>();

        public bool Exists(string i_OrderId)
        {
            return r_Orders.ContainsKey(i_OrderId);
        }

        public void Add(Order i_Order)
        {
            r_Orders.TryAdd(i_Order.OrderId, i_Order);
        }

        public Order? GetById(string i_OrderId)
        {
            if(r_Orders.TryGetValue(i_OrderId, out Order? order))
            {
                return order;
            }

            return null;
        }
    }
}
