using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Contracts
{
    public static class KafkaConstants
    {
        public const string OrdersTopic = "orders.topic";
        public const string DeadLetterTopic = "dlx_orders.topic";
    }
}
