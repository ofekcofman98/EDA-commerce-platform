1. 
Name: Ofek Cofman
ID: 209395524

2. 
APIs:

Producer (CartService)
  POST http://localhost:8080/create-order
  Body (JSON):
  {
    "orderId": "<string>",
    "numOfItems": <int>
  }

Consumer (OrderService)
GET http://localhost:8081/order-details?orderId=<ORDER_ID>


3.
The type of exchange I chose is fanout, 
because the order details must be broadcast to multiple downstream services,
so all the queues will recieve the message and all the services as well.

4. 
For a fanout exchange there is no meaningful binding key.
The queue is bound with an empty string ("") as the binding key, but fanout exchanges ignore the routing key and deliver every message to all bound queues.
So effectively there is no logical binding key used for routing.

5. 
The producer declares the exchange because ___.
Both services declare the queue because they are different applications in different projects, 
and neither of them can assume the queue exists.
