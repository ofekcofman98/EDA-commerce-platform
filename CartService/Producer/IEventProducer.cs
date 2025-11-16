using Shared.Contracts;

namespace CartService.Producer
{
    public interface IEventProducer
    {
        void PublishOrder(Order order);
    }
}