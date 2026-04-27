# Design URL Shortener (like bit.ly)

## Requirements
- Shorten long URLs to short aliases (e.g., bit.ly/abc123).
- Redirect short URLs to original URLs.
- High read throughput (100:1 read-to-write ratio).
- Analytics: click counts, geographic data.
- Custom aliases (optional).
- Link expiration (optional).

## Architecture

### Components
1. **API Service**: Handles create and redirect requests.
2. **URL Database**: Stores mapping of short code → original URL.
3. **Cache Layer**: Redis for hot URLs (most redirects hit a small set of popular URLs).
4. **Analytics Service**: Track clicks asynchronously via message queue.
5. **Load Balancer**: Distribute traffic across API instances.

### Short URL Generation Approaches
1. **MD5/SHA256 Hash + Base62**: Hash the URL, take first 7 characters, encode in base62. Collision handling needed.
2. **Counter-based (Snowflake ID)**: Auto-incrementing counter encoded in base62. No collisions but predictable.
3. **Pre-generated Key Service**: Generate unique keys offline, distribute to API servers. No collision risk, fast.

### Database Choice
- **Key-Value Store** (DynamoDB/Redis): Perfect for simple get/put operations.
- **SQL** (PostgreSQL): If you need complex queries, analytics, or transactions.

### Scaling Strategy
- Read replicas for database read scaling.
- Redis cache with high TTL for popular URLs.
- CDN for geographic distribution.
- Horizontal scaling of API servers behind load balancer.

## Capacity Estimation
- 100M new URLs/month → ~40 URLs/second write.
- 10B redirects/month → ~4000 redirects/second read.
- Storage: 100M × 500 bytes = 50 GB/month.
- Cache: Top 20% URLs = ~10 GB Redis.

## Key Trade-offs
- Hash-based vs counter-based ID generation.
- SQL vs NoSQL for storage.
- 301 (permanent) vs 302 (temporary) redirect for SEO implications.
