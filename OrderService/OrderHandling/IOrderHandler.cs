using Shared.Contracts.Events;
using System.Text.Json;

namespace OrderService.OrderHandling
{
    public interface IOrderEventHandler
    {
        EventType EventType { get; }
        void Handle(JsonElement i_Payload);
    }
}
