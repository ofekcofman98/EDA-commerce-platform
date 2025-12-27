using Shared.Contracts.Events;

namespace CartService.Producer
{
    public interface IEventProducer
    {
        Task PublishAsync(EventEnvelope eventEnvelope);
    }
}