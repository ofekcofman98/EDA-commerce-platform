(1) 
Name: Ofek Cofman
ID: 209395524

################################

(2) 

Topic Name: "orders.topic"

Its purpose is to facilitate communication between the Cart Service (producer) and the Order Service (consumer)
by routing order-related messages based on specific patterns.

################################

(3)

The key I used in the message is "orderId", unsuring:
all events for the same order go to the same partition.
Events are processed in the correct order.
Enables correct state transitions (for example, "create" first, and then "update").

################################

(4)
Error Handling:

In the Cart Service (producer):
1. Input validation for all order creation and update requests
2. Chain of Responsibility validators ensuring each order and item is structurally valid.
3. Kafka reliability settings:
	Acks=All
	EnableIdempotence=true
	RetryBackoffMs and batching configuration
4. Exceptions during publishing are handled and logged.
5. Invalid requests return structured HTTP 400 responses with detailed error messages.

In the Order Service (consumer):
1. Continuous consumption loop wrapped in a global try/catch.
2. Dedicated handling for:
	ConsumeException (Kafka-specific errors)
	JsonException (deserialization errors)
3. Each message is protected, meaning one bad message does not crash the consumer
4. Consumer is safely closed on cancellation (_consumer.Close())

Repository Integrity
1. Thread-safe storage using ConcurrentDictionary.
2. Additional synchronization with lock for HashSet updates
3. Ensures the service can safely handle concurrent ingestion from Kafka

Message Validation
1. Payload is validated before adding to repository.
2. Both OrderCreated and OrderUpdated events check for null content.




@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
Cart Service (Producer)
1. Input & Business Validation

Before producing any event to Kafka:

All incoming HTTP requests are validated.

A Chain of Responsibility verifies:

Order ID validity

Unique order ID

Valid number of items

Item quantity & price constraints

Non-negative totals and valid currency

Only fully valid orders are published.

2. Kafka Reliability Configuration

The producer is configured for exactly-once semantics at the Kafka level:

Acks = All ensures the leader + replicas confirm the write.

EnableIdempotence = true prevents duplicates during retries.

RetryBackoffMs and batching settings minimize publish failures.

MessageTimeoutMs prevents hanging producers.

This ensures that even under network instability, the system emits each event at most once and in order per key (orderId).

3. Exception Handling During Publishing

Publishing is wrapped in try/catch:

Serialization failures

Kafka broker errors

Connectivity issues

All are logged, ensuring visibility without impacting the HTTP API stability.

4. Structured HTTP Error Responses

Invalid requests return:

{
  "error": "Validation Failed",
  "detail": "OrderId must not be empty"
}


This keeps the API predictable and testable.

Order Service (Consumer)
1. Protected Consumption Loop

The entire background service loop is wrapped in:

global try/catch

message-specific try/catch

Ensuring:

One bad message never crashes the consumer

The service stays alive indefinitely

2. Dedicated Error Types

Handled separately for clarity:

ConsumeException — Kafka-specific failures

JsonException — malformed or mismatching payload

OperationCanceledException — graceful shutdown

General Exception — logged with topic, partition, offset, key

This gives full observability of event errors.

3. Safe Shutdown

On cancellation, the consumer calls:

_consumer.Close();


Which commits offsets and leaves the group cleanly.

Repository Integrity
1. Thread-Safe Storage Layer

All in-memory domain state is stored using:

ConcurrentDictionary<string, OrderDetails>

ConcurrentDictionary<string, HashSet<string>> for topic → orderId mapping
(with lock around HashSet writes)

Ensures the service is safe even under:

multiple handler executions

concurrent Kafka partition assignment

rapid ingestion rates

Message-Level Validation

Before updating domain state:

EventEnvelope is validated after deserialization

Handlers check for nulls or mismatching payload types

OrderUpdatedHandler ensures the order exists before applying updates

Invalid payloads are ignored gracefully

This prevents inconsistent or partial domain state.



################################


APIs:

Producer (CartService)
  POST http://localhost:8080/create-order
  Body (JSON):
{
  "orderId": "1234",
  "numOfItems": 3
}

 PUT http://localhost:8080/update-order
 Body (JSON):
 {
  "orderId": "1234",
  "OrderStatus": "Confirmed"
 }


Consumer (OrderService)
GET http://localhost:8081/order-details?orderId=1234

GET http://localhost:8081/order-details?topicName=orders.topic
