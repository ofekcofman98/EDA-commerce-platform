using CartService.Interfaces;
using Shared.Contracts;
using Shared.Contracts.Orders;

namespace CartService.Interfaces
{
    public interface IValidationFactory
    {
        IValidator<CreateOrderRequest> GetRequestChain();
        IValidator<Order> GetOrderChain();
        IValidator<Item> GetItemChain();
    }
}

