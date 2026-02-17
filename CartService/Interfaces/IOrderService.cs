using Shared.Contracts;

namespace CartService.Interfaces
{
    public interface IOrderService
    {
        Task<ServiceResponse> CreateNewOrder(CreateOrderRequest i_Request);
        Task<ServiceResponse> UpdateOrderStatus(UpdateOrderRequest i_Request);
    }
}

