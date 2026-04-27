# Message Queues and Event-Driven Architecture

## Overview
Message queues decouple producers from consumers, enabling asynchronous processing, load leveling, and fault tolerance. They are fundamental to building scalable distributed systems.

## Messaging Patterns

### Point-to-Point (Queue)
One producer, one consumer per message. Messages are consumed once and removed. Used for task distribution (e.g., job processing).

### Publish-Subscribe (Pub/Sub)
One producer, multiple consumers. Each subscriber receives a copy of the message. Used for event broadcasting (e.g., notifications, analytics).

### Event Sourcing
Store all state changes as an immutable sequence of events. Current state is derived by replaying events. Enables audit trails, temporal queries, and event replay.

### CQRS (Command Query Responsibility Segregation)
Separate read and write models. Commands mutate state, Queries read state. Often combined with event sourcing. Enables independent scaling of reads and writes.

## Tools and Trade-offs

### Apache Kafka
- High throughput, ordered within partitions, durable log.
- Best for: event streaming, log aggregation, real-time analytics.
- Consumer groups for parallel processing.
- Retention-based (keeps messages for configured period).

### RabbitMQ
- Rich routing (exchanges, bindings), supports multiple protocols.
- Best for: complex routing, RPC, task queues.
- Message acknowledgment with configurable delivery guarantees.
- Queue-based (messages deleted after consumption).

### Amazon SQS
- Fully managed, auto-scaling, no infrastructure to manage.
- Standard (at-least-once, best-effort ordering) vs FIFO (exactly-once, ordered).
- Dead Letter Queue for failed message handling.

## Key Concepts
- **At-most-once**: Fire and forget. May lose messages.
- **At-least-once**: Retry until acknowledged. May duplicate messages. Most common.
- **Exactly-once**: Hardest to achieve. Requires idempotent consumers or transactional messaging.
- **Dead Letter Queue**: Store failed messages for later investigation.
- **Backpressure**: Mechanism to slow producers when consumers can't keep up.

## Interview Tips
Discuss: message ordering guarantees, handling duplicate messages (idempotency), DLQ strategy, and choosing between Kafka and traditional queues based on use case.
