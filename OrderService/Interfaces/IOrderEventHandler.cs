using Shared.Contracts.Events;
using System.Text.Json;

namespace OrderService.Interfaces
{
    public interface IOrderEventHandler
    {
        EventType EventType { get; }
        void Handle(JsonElement i_Payload);
    }
}
