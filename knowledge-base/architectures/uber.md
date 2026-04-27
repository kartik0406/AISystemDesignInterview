# Design Uber (Ride-Sharing Platform)

## Requirements
- Match riders with nearby drivers in real-time.
- Real-time location tracking for drivers.
- ETA calculation and route optimization.
- Pricing (surge pricing during demand spikes).
- Payment processing.
- Trip history and receipts.

## Architecture

### Components
1. **API Gateway**: Mobile client requests routing.
2. **Rider Service**: Rider profiles, ride requests.
3. **Driver Service**: Driver profiles, availability, location updates.
4. **Matching Service**: Real-time rider-driver matching.
5. **Location Service**: Track and query driver locations.
6. **Pricing Service**: Dynamic pricing based on demand/supply.
7. **Trip Service**: Trip lifecycle management.
8. **Payment Service**: Process payments, split fares.
9. **Notification Service**: Push notifications for ride updates.

### Real-Time Location Tracking
- Drivers send location updates every 3-5 seconds.
- Use WebSocket or MQTT for persistent connections.
- Store in geospatial index (QuadTree, Geohash, or H3).
- Scale: millions of drivers × updates every few seconds.

### Matching Algorithm
1. Rider requests ride with pickup location.
2. Query geospatial index for nearby available drivers.
3. Consider: distance, ETA, driver rating, vehicle type.
4. Send ride offer to best match driver.
5. Driver accepts/rejects within timeout.
6. If rejected, offer to next best match.

### Geospatial Indexing
- **Geohash**: Encode lat/lng into string prefix. Proximity = common prefix. Used with Redis or database.
- **QuadTree**: Recursive spatial decomposition. Dynamic cells based on density.
- **H3 (Uber's choice)**: Hexagonal hierarchical indexing. Uniform distance properties.

### Surge Pricing
- Monitor demand (ride requests) vs supply (available drivers) per region.
- Apply multiplier when demand exceeds supply.
- Use geohash-based regions for localized pricing.
- Balance: extract value while maintaining rider satisfaction.

### Database Design
- **User/Driver profiles**: PostgreSQL.
- **Real-time locations**: Redis with geospatial commands (GEOADD, GEORADIUS).
- **Trip data**: PostgreSQL with time-based partitioning.
- **Analytics**: Kafka → data warehouse.

## Key Trade-offs
- Accuracy vs speed in matching (nearest vs best match).
- Real-time vs eventual consistency for location data.
- Push vs pull model for driver location updates.
