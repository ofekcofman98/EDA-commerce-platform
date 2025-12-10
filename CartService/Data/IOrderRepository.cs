using Shared.Contracts;

namespace CartService.Data
{
    public interface IOrderRepository
    {
        bool Exists(string orderId);
        void Add(Order order);
    }
}
