using Shared.Contracts;

namespace CartService.OrderUpdate
{
    public interface IOrderUpdateService
    {
        Task<ServiceResponse> UpdateOrderStatus(UpdateOrderRequest i_Request);
    }
}
