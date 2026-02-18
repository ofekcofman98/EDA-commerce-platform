using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared.Contracts.Orders;

namespace Shared.Contracts
{
    public class UpdateOrderRequest
    {
        public required string OrderId { get; set; }
        //public OrderStatus OrderStatus { get; set; }
        public required string Status { get; set; }
    }
}
