# Database Sharding

## Overview
Sharding is a horizontal partitioning strategy that distributes data across multiple database instances. Each shard holds a subset of the data, enabling the system to scale beyond the capacity of a single machine.

## Sharding Strategies

### Hash-Based Sharding
Apply a hash function to a key (e.g., user_id) to determine the shard. Provides uniform distribution but makes range queries difficult. Consistent hashing variant reduces data movement during rebalancing.

### Range-Based Sharding
Partition data based on value ranges (e.g., users A-M on shard 1, N-Z on shard 2). Supports range queries but can lead to hotspots if data distribution is uneven.

### Geographic Sharding
Partition based on geographic region. Reduces latency for users by keeping data close. Useful for applications with strong locality patterns (e.g., Uber, food delivery).

### Directory-Based Sharding
Maintain a lookup table mapping keys to shards. Most flexible but introduces a single point of failure (the directory).

## Consistent Hashing
Maps both data keys and server nodes onto a hash ring. When a server is added/removed, only keys adjacent to the change point need remapping. Virtual nodes improve balance. Used by: DynamoDB, Cassandra, Memcached.

## Challenges
- **Cross-shard queries**: Joins across shards are expensive. Design schema to minimize cross-shard operations.
- **Rebalancing**: Adding/removing shards requires data migration. Consistent hashing minimizes this.
- **Transactions**: Distributed transactions across shards are complex. Consider eventual consistency or saga pattern.
- **Hotspots**: Uneven data distribution overloads specific shards. Monitor and rebalance proactively.

## Interview Tips
Discuss: choice of shard key (high cardinality, even distribution), handling cross-shard queries, rebalancing strategy, and impact on application code.
