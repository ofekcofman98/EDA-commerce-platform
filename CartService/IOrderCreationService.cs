using Shared.Contracts;

namespace CartService
{
    public interface IOrderCreationService
    {
        ServiceResponse CreateNewOrder(CreateOrderRequest i_Request);
    }
}
