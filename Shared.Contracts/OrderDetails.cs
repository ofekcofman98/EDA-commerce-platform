using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Contracts
{
    public class OrderDetails
    {
        public Order Order { get; set; }
        public decimal shippingCost { get; set; }

        public OrderDetails(Order i_Order, decimal i_ShippingCost)
        {
            this.Order = i_Order;
            this.shippingCost = i_ShippingCost;
        }
    }
}
