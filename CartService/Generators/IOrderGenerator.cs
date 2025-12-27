using Shared.Contracts;
using Shared.Contracts.Orders;

namespace CartService.Generators
{
    public interface IOrderGenerator
    {
        Order GenerateOrder(CreateOrderRequest i_Request, List<Item> items);
    }
}
