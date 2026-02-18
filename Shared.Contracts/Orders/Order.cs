using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Shared.Contracts.Orders
{
    public class Order
    {
        public required string OrderId { get; set; }
        public required string CustomerId { get; set; }
        public DateTime OrderDate { get; set; }
        public required List<Item> Items { get; set; }
        public decimal TotalAmount { get; set; }
        public required string Currency { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public OrderStatus Status { get; set; }
        
        public Order() { }

        [System.Diagnostics.CodeAnalysis.SetsRequiredMembers]
        public Order(CreateOrderRequest i_Request)
        {
            OrderId = i_Request.orderId;
            CustomerId = string.Empty;
            Items = new List<Item>();
            Currency = string.Empty;
        }
    }
}
