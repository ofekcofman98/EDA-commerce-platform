using Shared.Contracts.Orders;

namespace OrderService.Data
{
    public interface IOrderRepository
    {
        void Add(OrderDetails i_OrderDetails);
        void AddOrderToTopic(string i_TopicName, string i_OrderId);
        IEnumerable<string> GetAllOrderIdsFromTopic(string i_TopicName);
        OrderDetails? GetById(string i_OrderId);

    }
}
