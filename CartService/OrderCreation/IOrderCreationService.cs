using Shared.Contracts;

namespace CartService.OrderCreation
{
    public interface IOrderCreationService
    {
        ServiceResponse CreateNewOrder(CreateOrderRequest i_Request);
    }
}
