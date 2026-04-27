# Microservices Architecture

## Overview
Microservices is an architectural style where an application is composed of small, independently deployable services. Each service owns its data and communicates via well-defined APIs.

## Key Principles
- **Single Responsibility**: Each service does one thing well.
- **Independence**: Services can be developed, deployed, and scaled independently.
- **Decentralized Data**: Each service owns its database (Database per Service pattern).
- **Fault Isolation**: Failure in one service doesn't cascade to others.

## Communication Patterns

### Synchronous (Request-Response)
- REST/HTTP: Simple, widely supported, stateless.
- gRPC: High performance, binary protocol, bidirectional streaming. Good for internal service-to-service.

### Asynchronous (Event-Driven)
- Message queues: Decouple services, handle spikes, ensure eventual processing.
- Event bus: Broadcast events for multiple consumers (Pub/Sub).

## Service Discovery
- **Client-side**: Client queries service registry (e.g., Eureka) and selects instance.
- **Server-side**: Load balancer queries registry (e.g., AWS ALB + ECS).
- **DNS-based**: Kubernetes services use DNS for discovery.

## API Gateway Pattern
Single entry point for all client requests. Responsibilities:
- Request routing to appropriate microservice.
- Authentication and authorization.
- Rate limiting and throttling.
- Request/response transformation.
- Circuit breaker integration.

## Saga Pattern (Distributed Transactions)
Manage data consistency across services without distributed transactions:
- **Choreography**: Each service publishes events, others react. Simple but hard to debug.
- **Orchestration**: Central coordinator manages the saga. Easier to understand and maintain.

## Circuit Breaker Pattern
Prevent cascade failures:
- **Closed**: Normal operation, requests pass through.
- **Open**: Too many failures, requests fail fast without calling downstream.
- **Half-Open**: Test with limited requests to check if service recovered.

## Interview Tips
Discuss: why microservices over monolith for this use case, data consistency challenges, inter-service communication trade-offs, and deployment strategies (blue-green, canary).
