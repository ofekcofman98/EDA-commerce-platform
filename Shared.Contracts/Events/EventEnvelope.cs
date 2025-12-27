using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Shared.Contracts.Events
{
    public class EventEnvelope
    {
        public EventType EventType { get; set; }
        public string OrderId { get; set; }
        public JsonElement Payload { get; set; }
    }
}
