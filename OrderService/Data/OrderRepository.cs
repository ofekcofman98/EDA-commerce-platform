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

            //decimal shippingCost = ShippingCostService.CalculateShippingCost(i_Order);
            //OrderDetails orderDetails = new OrderDetails(i_Order, shippingCost);
            //OrderDetailsList.Add(orderDetails);
        }
        
        public OrderDetails? GetById(string i_OrderId)
        {
            if (_orderDetailsMap.TryGetValue(i_OrderId, out OrderDetails? o_OrderDetails))
            {
                return o_OrderDetails;
            }

            return null;
        }
    }
}
