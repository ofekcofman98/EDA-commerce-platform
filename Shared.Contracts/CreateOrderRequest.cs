using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Contracts
{
    public class CreateOrderRequest
    {
        public required string orderId { get; set; }
        public int? numOfItems { get; set; }
    }
}
