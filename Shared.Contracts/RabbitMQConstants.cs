using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Contracts
{
    public static class RabbitMQConstants
    {
        public static class Exchange
        {
            public const string Orders = "orders.exchange";
            public const string DeadLetter = "dlx_orders.exchange";
        }

        public static class Queue
        {
            public const string Orders = "orders.queue";
            public const string DeadLetters = "orders.queue.dead";
        }

        public static class RoutingKey
        {
            public const string OrderCreated = "order.created";
            public const string OrderUpdated = "order.updated";

            public const string AllOrders = "order.*";
        }

        public static class BindingKey
        {
            public const string OrderCreated = "order.created";
            public const string OrderUpdated = "order.updated";

            public const string AllOrders = "order.*";
        }

    }
}
