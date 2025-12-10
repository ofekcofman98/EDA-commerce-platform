using Shared.Contracts;

namespace CartService.Generators
{
    public interface IOrderGenerator
    {
        Order GenerateOrder(CreateOrderRequest i_Request, List<Item> items);
    }
}
