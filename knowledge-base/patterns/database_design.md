# Database Design Patterns

## Overview
Choosing the right database and designing an effective schema is critical for system performance, scalability, and maintainability.

## SQL vs NoSQL Trade-offs

### SQL (Relational)
- ACID transactions, strong consistency, complex queries with JOINs.
- Best for: structured data, complex relationships, financial systems.
- Examples: PostgreSQL, MySQL, Oracle.
- Scaling: vertical primarily, read replicas for read scaling, sharding for write scaling.

### NoSQL Document Stores
- Flexible schema, nested documents, horizontal scaling.
- Best for: content management, user profiles, catalogs.
- Examples: MongoDB, CouchDB.
- Trade-off: limited JOIN support, eventual consistency.

### NoSQL Key-Value Stores
- Simple get/put operations, extremely fast, high throughput.
- Best for: caching, session storage, user preferences.
- Examples: Redis, DynamoDB, Memcached.
- Trade-off: limited query capabilities.

### NoSQL Wide-Column Stores
- Column families, efficient for sparse data, write-optimized.
- Best for: time series, IoT data, analytics.
- Examples: Cassandra, HBase, BigTable.
- Trade-off: complex data modeling, limited ad-hoc queries.

### Graph Databases
- Nodes and edges, efficient traversal of relationships.
- Best for: social networks, recommendation engines, fraud detection.
- Examples: Neo4j, Amazon Neptune.
- Trade-off: not suitable for non-graph workloads.

## Indexing Strategies
- **B-Tree**: Default for most databases. Good for range queries and equality.
- **Hash Index**: Fast equality lookups. No range support.
- **Composite Index**: Multiple columns. Order matters for query optimization.
- **Covering Index**: Includes all query columns. Avoids table lookup.
- **Full-text Index**: For text search. Inverted index with tokenization.

## Replication
- **Master-Slave**: One write node, multiple read replicas. Read scaling.
- **Multi-Master**: Multiple write nodes. Write scaling but conflict resolution needed.
- **Synchronous**: Strong consistency but higher latency.
- **Asynchronous**: Lower latency but potential data loss on failure.

## Interview Tips
Discuss: schema design decisions with justification, indexing strategy based on query patterns, replication mode based on consistency requirements, and when to denormalize.
