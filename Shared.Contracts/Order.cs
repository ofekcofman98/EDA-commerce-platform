using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Shared.Contracts
{
    public class Order
    {
        public string OrderId { get; private set; }
        public string CustomerId { get; set; }
        public DateTime OrderDate { get; set; }
        public List<Item> Items { get; set; }
        public decimal TotalAmount { get; set; }//=> Items?.Sum(item => item.quantity * item.price) ?? 0;
        public string Currency { get; set; }
        
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public OrderStatus Status { get; set; }
        public Order(CreateOrderRequest i_Request)
        {
            OrderId = i_Request.orderId;
        }
    }
}
