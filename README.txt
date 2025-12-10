(1) 
Name: Ofek Cofman
ID: 209395524

################################

(2) 
APIs:

Producer (CartService)
  POST http://localhost:8080/create-order
  Body (JSON):
{
  "orderId": "1234",
  "numOfItems": 3
}

Consumer (OrderService)
GET http://localhost:8081/order-details?orderId=1234


################################

(3)
A topic exchange supports both broadcasting the event to multiple consumers and filtering based on routing patterns.
It forwards each message to all queues whose binding key matches the routing key.
This allows multiple downstream services to receive the order event, while still allowing each consumer to filter only the messages relevant to it.
Since the Order Service should receive only new orders, its binding pattern must match routing keys that represent new-order events.


################################

(4)
Yes, there is a binding key on the consumer: "#.new".
The producer publishes all new-order events using the routing key: "order.new"
The binding key "#.new" matches any routing key that ends with ".new",
so the consumer receives only new order events, and ignores any other types of events that may be added in the future.

################################

(5)
The producer declares the exchange because it is responsible for delivering the message, and it needs the exchange to publish the message.
Both services declare the queue, because each application is deployed independently and cannot assume that the queue already exists.
Queue declaration in RabbitMQ is idempotent, so declaring it from both sides is safe and ensures the queue always exists for both publishing and consuming.