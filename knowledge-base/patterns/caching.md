# Caching Patterns

## Overview
Caching is a technique to store frequently accessed data in a fast-access storage layer to reduce latency and database load. It is one of the most critical components in system design for achieving scalability.

## Key Caching Strategies

### Cache-Aside (Lazy Loading)
Application checks cache first. On miss, fetches from database, then populates cache. Best for read-heavy workloads. Risk: stale data if database is updated directly.

### Write-Through
Every write goes to both cache and database synchronously. Ensures cache consistency but increases write latency. Good for applications requiring strong consistency.

### Write-Behind (Write-Back)
Writes go to cache first, then asynchronously written to database. Improves write performance but risks data loss if cache fails before persistence. Use with durable cache systems.

### Read-Through
Cache sits between application and database. On miss, cache itself fetches from database. Simplifies application code.

## Cache Invalidation Strategies
- **TTL-based**: Set expiration time on cached entries. Simple but may serve stale data.
- **Event-driven**: Invalidate on database change events. More complex but ensures freshness.
- **Version-based**: Each entry has a version number. Compare versions to detect staleness.

## Common Pitfalls
- Cache stampede: Multiple simultaneous cache misses causing database overload. Solution: mutex lock or request coalescing.
- Hot key problem: Single popular key overwhelms one cache node. Solution: replicate hot keys across nodes.
- Memory pressure: Cache grows unbounded. Solution: LRU/LFU eviction policies.

## Tools
- Redis: In-memory data structure store, supports complex data types, pub/sub, persistence.
- Memcached: Simple key-value cache, multi-threaded, good for simple caching needs.
- CDN: Content Delivery Network for caching static assets at edge locations.

## Interview Tips
Interviewers look for: understanding of cache invalidation trade-offs, awareness of consistency issues, ability to estimate cache hit rates, and knowledge of eviction policies.
