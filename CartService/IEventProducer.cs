using Shared.Contracts;

namespace CartService
{
    public interface IEventProducer
    {
        void PublishOrder(Order order);
    }
}