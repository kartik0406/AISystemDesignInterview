# CAP Theorem and Consistency Patterns

## CAP Theorem
In a distributed system, you can only guarantee two of three properties simultaneously:
- **Consistency (C)**: Every read receives the most recent write.
- **Availability (A)**: Every request receives a response (success or failure).
- **Partition Tolerance (P)**: System continues operating despite network partitions.

Since network partitions are inevitable in distributed systems, the real choice is between **CP** (consistency during partitions) and **AP** (availability during partitions).

## PACELC Theorem (Extension of CAP)
If there is a **P**artition, choose **A**vailability or **C**onsistency. **E**lse, when running normally, choose **L**atency or **C**onsistency.

Examples:
- **PA/EL**: Cassandra, DynamoDB — available during partitions, low latency normally.
- **PC/EC**: HBase, BigTable — consistent always, higher latency.
- **PA/EC**: MongoDB (default config) — available during partitions, consistent normally.

## Consistency Models

### Strong Consistency (Linearizability)
Every read returns the latest write. Achieved via consensus protocols (Paxos, Raft). Used by: Google Spanner, CockroachDB. Trade-off: higher latency.

### Eventual Consistency
Writes propagate asynchronously. Reads may return stale data temporarily, but eventually all replicas converge. Used by: DynamoDB, Cassandra, S3. Trade-off: stale reads possible.

### Causal Consistency
Preserves cause-and-effect ordering. If operation A caused operation B, everyone sees A before B. Weaker than strong but stronger than eventual. Used by: MongoDB (causal sessions).

### Read-Your-Writes Consistency
After a write, the writing client always sees their own write. Others may see stale data. Common implementation: route reads to the same node that handled the write.

## Consensus Protocols
- **Paxos**: Classic consensus algorithm. Complex to implement. Used in Google Chubby.
- **Raft**: Simplified consensus. Leader-based. Used in etcd, CockroachDB.
- **ZAB**: Zookeeper Atomic Broadcast. Used in Apache Zookeeper.

## Interview Tips
Demonstrate understanding of: when to choose CP vs AP, real-world examples of each, PACELC for more nuanced discussion, and how consistency affects user experience.
