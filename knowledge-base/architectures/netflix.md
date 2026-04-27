# Design Netflix (Video Streaming Platform)

## Requirements
- Stream video content to millions of concurrent users.
- Support multiple devices and quality levels.
- Content recommendation engine.
- Upload and process new content (encoding).
- User profiles, watch history, and preferences.

## Architecture

### Components
1. **API Gateway**: Routes requests, handles auth.
2. **User Service**: Authentication, profiles, preferences.
3. **Content Service**: Metadata, catalog browsing, search.
4. **Streaming Service**: Adaptive bitrate streaming (ABR).
5. **Recommendation Engine**: ML-based personalized recommendations.
6. **Transcoding Pipeline**: Convert uploaded videos to multiple formats/qualities.
7. **CDN**: Edge servers for low-latency content delivery.

### Video Processing Pipeline
1. Upload raw video to object storage (S3).
2. Trigger transcoding jobs (multiple resolutions: 240p to 4K).
3. Segment videos into small chunks (2-10 seconds each).
4. Generate manifest files (HLS/DASH).
5. Push to CDN edge locations.

### Adaptive Bitrate Streaming
- Client monitors bandwidth and buffer.
- Switches between quality levels seamlessly.
- Protocols: HLS (Apple), DASH (open standard).

### Recommendation System
- Collaborative filtering: Users who watched X also watched Y.
- Content-based filtering: Similar genres, actors, directors.
- Hybrid approach with ML models.
- A/B testing for recommendation algorithms.

### Database Design
- **User data**: SQL (PostgreSQL) for ACID transactions.
- **Content metadata**: Document store (MongoDB) for flexible schema.
- **View history**: Wide-column (Cassandra) for time-series write throughput.
- **Recommendations**: Redis for fast cached recommendations.

## Scaling Challenges
- Peak traffic (evenings/weekends) can be 3-5x normal.
- CDN is the primary scaling mechanism for content delivery.
- Microservices architecture for independent scaling.
- Pre-compute recommendations offline, serve from cache.

## Key Trade-offs
- Pre-transcoding all formats vs on-demand transcoding.
- CDN cost vs latency.
- Real-time vs batch recommendation updates.
