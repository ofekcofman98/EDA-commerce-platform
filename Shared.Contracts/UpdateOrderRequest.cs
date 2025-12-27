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
        public string OrderId { get; set; }
        public OrderStatus OrderStatus { get; set; }
    }
}
