using Shared.Contracts.Orders;

namespace OrderService.Data
{
    public interface IOrderRepository
    {
        void Add(OrderDetails i_OrderDetails);
        object GetAllOrderIdsFromTopic(string topicName);
        OrderDetails? GetById(string i_OrderId);

    }
}