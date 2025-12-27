using Shared.Contracts;
using Shared.Contracts.Orders;

namespace CartService.Generators
{
    public class BaseOrderGenerator : IOrderGenerator
    {
        public virtual Order GenerateOrder(CreateOrderRequest i_Request, List<Item> i_Items)
        {
            //CreateOrderRequest i_Request = new CreateOrderRequest();
            
            Order order = new Order(i_Request);

            order.Items = i_Items;
            order.TotalAmount = order.Items.Sum(item => item.price * item.quantity);
            order.Status = OrderStatus.New;
            order.OrderDate = DateTime.UtcNow;

            return order;
        }
    }
}
