using Shared.Contracts;

namespace OrderService.Data
{
    public interface IOrderRepository
    {
        void Add(OrderDetails i_OrderDetails);
        OrderDetails? GetById(string i_OrderId);

    }
}