using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Contracts
{
    public static class RabbitMQConstants
    {
        public const string ExchangeName = "orders.exchange";
        public const string QueueName = "orders.queue";
        public const string DeadLetterExchangeName = "dlx_orders.exchange";

    }
}
