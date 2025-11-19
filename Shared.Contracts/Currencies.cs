using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Contracts
{
    public class Currencies
    {
        public static readonly HashSet<string> s_AllowedCurrencies = new HashSet<string>
        {
            "USD", "EUR", "ILS"
        };

    }
}
