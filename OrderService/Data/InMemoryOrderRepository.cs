using OrderService.Interfaces;
using Shared.Contracts.Orders;
using System.Collections.Concurrent;
using System.Linq;

namespace OrderService.Data
{
    public class InMemoryOrderRepository : IOrderRepository
    {
        private readonly ILogger<InMemoryOrderRepository> _logger;

        private readonly ConcurrentDictionary<string, OrderDetails> _orderDetailsMap = new();
        private readonly ConcurrentDictionary<string, HashSet<string>> _topicOrders = new();

        public InMemoryOrderRepository(ILogger<InMemoryOrderRepository> logger)
        {
            _logger = logger;
        }
        
        public void Add(OrderDetails i_OrderDetails)
        {
            _orderDetailsMap.TryAdd(i_OrderDetails.Order.OrderId, i_OrderDetails);
        }

        public void AddOrderToTopic(string i_TopicName, string i_OrderId)
        {
            _topicOrders.AddOrUpdate(i_TopicName,
                addValue: new HashSet<string> { i_OrderId },
                updateValueFactory: (key, existingSet) =>
                {
                    lock (existingSet)
                    {
                        existingSet.Add(i_OrderId);
                    }

                    return existingSet;
                });
        }

        public IEnumerable<string> GetAllOrderIdsFromTopic(string i_TopicName)
        {
            if (_topicOrders.TryGetValue(i_TopicName, out HashSet<string>? orderIds))
            {
                _logger.LogInformation("Found {Count} orders for topic {TopicName}", orderIds.Count, i_TopicName);
                return orderIds;
            }

            _logger.LogWarning("Topic lookup failed: The topic '{TopicName}' does not exist in the repository.", i_TopicName);

            return Array.Empty<string>();
        }


        public OrderDetails? GetById(string i_OrderId)
        {
            if (_orderDetailsMap.TryGetValue(i_OrderId, out OrderDetails? o_OrderDetails))
            {
                return o_OrderDetails;
            }


            return null;
        }

        public void PrintAllOrderIds()
        {
            Console.WriteLine("All orders:");
            if (_orderDetailsMap != null)
            {
                foreach (var od in _orderDetailsMap.Values)
                {
                    Console.WriteLine($"orderId: {od.Order.OrderId}");
                }
            }

        }
    }
}
