using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Shared.Contracts
{
    public class Item
    {
        public string itemId { get; set; }
        public int quantity { get; set; }
        public decimal price { get; set; }    
    }
}
