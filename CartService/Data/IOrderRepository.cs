using Shared.Contracts.Orders;

namespace CartService.Data
{
    public interface IOrderRepository
    {
        bool Exists(string orderId);
        void Add(Order order);
        Order? GetById(string i_OrderId);

    }
}
