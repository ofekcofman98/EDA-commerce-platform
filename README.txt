(1) 
Name: Ofek Cofman
ID: 209395524

(Instructions and APIs at the end)
################################

(2) 

Topic Name: "orders.topic"

Its purpose is to facilitate communication between the Cart Service (producer) and the Order Service (consumer)
by routing order-related messages based on specific patterns.

################################

(3)

The key I used in the message is "orderId".

Reasoning: 
- Partition Affinity: Using orderId as the key ensures that all messages related to the same order are routed to the same Kafka partition.
- Strict Ordering: Since Kafka guarantees order within a partition, 
  this ensures that an "Order Created" event is always processed before an "Order Updated" event for any specific order.
- Concurrency Safety: It prevents race conditions where an update might try to modify an order that hasn't been created in the repository yet.

################################

(4)
Error Handling:

- In the Cart Service (producer):
	1. Input Validation: Comprehensive validation for all order creation and update requests before processing.

	2. Chain of Responsibility: A design pattern used to ensure each order and item is structurally and logically valid
	   (e.g., price, quantity, unique IDs) before being sent to the broker.
	
	3. Kafka reliability settings:
		Acks=All
		EnableIdempotence=true
		RetryBackoffMs and batching configuration
	
	4. Exception Management: All exceptions during the publishing phase are caught,
	   logged via ILogger, and return a structured error response to the client.


- In the Order Service (consumer):
	1. Resilient Consumption Loop: The main loop is wrapped in a global try/catch.
	   Fault Tolerance: Invalid messages (e.g., malformed JSON) are caught,logged as a Warning, and skipped. 
	   This prevents a single "poison pill" message from crashing the entire service. to the next message.
	
	2. Dedicated Exception Handling: 
	   * ConsumeException: For Kafka-specific connectivity or broker issues.
	   * JsonException: For deserialization failures, ensuring the consumer moves to the next offset.
	
	3. Thread-Safe Repository Integrity:
	   * Uses ConcurrentDictionary and lock mechanisms on HashSet updates.
	   * This ensures data integrity even when multiple events (Create/Update) 
	     for different orders are being processed rapidly or when the API and Consumer access the state simultaneously.
	
	4. Graceful Shutdown: Implements _consumer.Close() during service cancellation. 
	   This ensures that offsets are committed correctly and the consumer group rebalances cleanly, 
	   preventing message duplication upon restart.


################################

Instructions:
1. docker compose -f docker-compose.cartservice.yml up -d
   (CartService + Kafka + Zookeeper, APIs on port 8080)

2. docker compose -f docker-compose.orderservice.yml up -d
   (OrderService consumer, APIs on port 8081)


Note:
CartService docker-compose must be started first, as it creates the shared Docker network and Kafka broker.


Logs:
docker compose -f docker-compose.cartservice.yml logs -f
docker compose -f docker-compose.orderservice.yml logs -f



APIs:

Producer (CartService) - port 8080
  1.
  POST http://localhost:8080/create-order
  Body (JSON):
  {
    "orderId": "1234",
    "numOfItems": 3
  }
  
  2. 
  PUT http://localhost:8080/update-order
  Body (JSON):
  {
   "orderId": "1234",
   "status": "Confirmed"
  }
  
  Supported values:
    New,
    Pending,
	Paid,
    Confirmed,
    Shipped,
    Cancelled
	

Consumer (OrderService)
GET http://localhost:8081/order-details?orderId=1234

GET http://localhost:8081/getAllOrderIdsFromTopic?topicName=orders.topic
