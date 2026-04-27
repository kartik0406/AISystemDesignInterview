# Load Balancing

## Overview
Load balancing distributes incoming traffic across multiple servers to ensure no single server is overwhelmed. It improves availability, reliability, and responsiveness.

## Types of Load Balancers

### L4 (Transport Layer)
Operates at TCP/UDP level. Fast, based on IP and port. Cannot inspect application content. Examples: AWS NLB, HAProxy in TCP mode.

### L7 (Application Layer)
Operates at HTTP level. Can inspect headers, cookies, URLs for routing decisions. Supports content-based routing, SSL termination. Examples: AWS ALB, Nginx, HAProxy in HTTP mode.

## Algorithms
- **Round Robin**: Distributes requests sequentially. Simple but ignores server load.
- **Weighted Round Robin**: Assigns weights based on server capacity.
- **Least Connections**: Routes to server with fewest active connections. Good for long-lived connections.
- **IP Hash**: Consistent routing based on client IP. Ensures session affinity.
- **Random**: Simple random selection. Surprisingly effective at scale.

## Health Checks
Load balancers periodically check backend servers. Types:
- TCP health check: Can server accept connections?
- HTTP health check: Does /health return 200?
- Custom: Application-specific health criteria.

## High Availability
- Active-passive: Standby LB takes over on failure.
- Active-active: Multiple LBs share traffic via DNS round-robin.
- Floating IP / Virtual IP: Shared IP address that moves between LB instances.

## Common Patterns
- SSL termination at LB to offload encryption from backends.
- Sticky sessions for stateful applications (use with caution).
- Connection draining: Gracefully handle in-flight requests during server removal.

## Interview Tips
Discuss: L4 vs L7 trade-offs, health check strategies, global vs regional load balancing, and handling SSL/TLS.
