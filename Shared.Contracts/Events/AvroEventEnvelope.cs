using Avro;
using Avro.Specific;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Contracts.Events
{
    public class AvroEventEnvelope : ISpecificRecord
    {
        public required string EventType { get; set; }
        public required string OrderId { get; set; }
        public required string Payload { get; set; }

        [System.Text.Json.Serialization.JsonIgnore]
        public virtual Schema Schema => Schema.Parse(
            @"{
                ""namespace"": ""Shared.Contracts.Events"",
                ""type"": ""record"",
                ""name"": ""AvroEventEnvelope"",
                ""fields"": [
                    { ""name"": ""EventType"", ""type"": ""string"" },
                    { ""name"": ""OrderId"", ""type"": ""string"" },
                    { ""name"": ""Payload"", ""type"": ""string"" }
                ]
            }");

        public virtual object Get(int fieldPos)
        {
            switch (fieldPos)
            {
                case 0: return this.EventType;
                case 1: return this.OrderId;
                case 2: return this.Payload;
                default: throw new AvroRuntimeException("Bad index " + fieldPos + " in Get()");
            }
        }

        public virtual void Put(int fieldPos, object fieldValue)
        {
            switch (fieldPos)
            {
                case 0: this.EventType = (string)fieldValue; break;
                case 1: this.OrderId = (string)fieldValue; break;
                case 2: this.Payload = (string)fieldValue; break;
                default: throw new AvroRuntimeException("Bad index " + fieldPos + " in Put()");
            }
        }
    }
}
