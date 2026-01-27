using Shared.Contracts;

namespace CartService.OrderCreation
{
    public interface IOrderCreationService
    {
        Task<ServiceResponse> CreateNewOrder(CreateOrderRequest i_Request);
    }
}
