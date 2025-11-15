using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Contracts
{
    public class ServiceResponse
    {
        public bool IsSuccesful { get; set; }
        public string? ErrorMessage { get; set; }
        public Order? Order { get; set; }
        public string? OrderId { get; set; }
    }
}
