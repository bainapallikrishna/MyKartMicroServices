# MyKart Microservices - Caching Implementation Guide

## Overview

This solution implements a **distributed caching layer** using **Redis** across all microservices and the API Gateway. The implementation uses a **declarative, attribute-based approach** for clean and maintainable code.

## Architecture

### Caching Components

1. **CacheableAttribute** - Marks GET endpoints for caching
2. **InvalidateCacheAttribute** - Marks write endpoints (POST, PUT, DELETE) for cache invalidation
3. **CachingActionFilter** - Intercepts requests and handles cache logic
4. **CacheKeyHelper** - Utility for consistent cache key generation
5. **ICacheService** - Interface for cache operations
6. **CacheService** - Implementation using IDistributedCache
7. **RedisExtensions** - DI registration for Redis
8. **CachingExtensions** - DI registration for caching filter

## Usage

### For GET Endpoints (Cacheable)

```csharp
[HttpGet]
[Cacheable(durationInSeconds: 300)]  // Cache for 5 minutes
public JsonResult GetAllProducts()
{
	// Your code here
}

[HttpGet("{id}")]
[Cacheable(durationInSeconds: 600)]  // Cache for 10 minutes
public JsonResult GetProductById(string id)
{
	// Your code here
}
```

### For Write Endpoints (Cache Invalidation)

```csharp
[HttpPost]
[InvalidateCache("product:*")]
public JsonResult AddNewProduct(Product product)
{
	// Your code here - cache is invalidated after execution
}

[HttpPut]
[InvalidateCache("product:*", "inventory:*")]
public JsonResult UpdateProduct(Product product)
{
	// Multiple cache patterns can be invalidated
}

[HttpDelete]
[InvalidateCache("product:*")]
public JsonResult DeleteProduct(string id)
{
	// Your code here
}
```

## Cache Key Generation

Cache keys are generated using the following pattern:

```
mykart:<controller>:<action>:<parameters>
```

**Examples:**
- `mykart:product:getallproducts`
- `mykart:user:getuserbyid:john@example.com`
- `mykart:category:getcategorybyid:5`

## Implementation Details

### Configuration

1. **Redis Connection String**
   - **Local Development**: `localhost:6379`
   - **Docker Environment**: `redis:6379`

2. **Cache Instance Name**: `MyKart` (used in Redis)

3. **Default Cache Expiry**: 300 seconds (5 minutes)

### Microservices Updated

✅ **UserMicroservices**
- GET endpoints cached
- POST/PUT/DELETE invalidate cache

✅ **ProductMicroservices**
- GET endpoints cached (includes GetPrice)
- POST/PUT/DELETE/UpdateQuantity invalidate cache

✅ **CategoryMicroservices**
- GET endpoints cached
- POST/PUT/DELETE invalidate cache

✅ **PurchaseMicroservices**
- GET endpoints cached
- POST/PUT/DELETE/AddPurchase invalidate cache
- PurchaseProduct endpoint invalidates both purchase and product cache

## Configuration Files

All microservices have been updated with Redis connection strings:

### appsettings.json (Local)
```json
"ConnectionStrings": {
  "Redis": "localhost:6379"
}
```

### appsettings.Docker.json (Container)
```json
"ConnectionStrings": {
  "Redis": "redis:6379"
}
```

## Cache Invalidation Strategy

The implementation uses **pattern-based invalidation**:

- **User Service**: Invalidates all keys matching `user:*`
- **Product Service**: Invalidates all keys matching `product:*`
- **Category Service**: Invalidates all keys matching `category:*`
- **Purchase Service**: Invalidates `purchase:*` and optionally `product:*` (for PurchaseProduct)

## Performance Benefits

✅ **Reduced Database Load** - Frequently accessed data served from cache
✅ **Faster Response Times** - In-memory cache access (< 1ms typically)
✅ **Improved Scalability** - Less pressure on database servers
✅ **5-Minute Default TTL** - Balance between freshness and performance

## Running the Solution

### Prerequisites

1. **Redis Server** running on `localhost:6379` (or configured Docker service)
2. **SQL Server** with databases configured
3. **.NET 8 SDK**

### Local Development

```powershell
# Start Redis (if using Docker)
docker run -d -p 6379:6379 redis:alpine

# Build solution
dotnet build

# Run individual services
dotnet run --project UserMicroservices
dotnet run --project ProductMicroservices
dotnet run --project CategoryMicroservices
dotnet run --project PurchaseMicroservices
```

### Docker Compose

```bash
docker-compose up -d
```

## Troubleshooting

### Cache Not Working?
1. Verify Redis is running: `redis-cli ping` (should return PONG)
2. Check connection string in appsettings.json
3. Verify `ICacheService` is registered in DI container

### Cache Not Invalidating?
1. Ensure `[InvalidateCache]` attribute is applied to write operations
2. Check cache key patterns match expected keys
3. Review logs for cache invalidation errors

### Performance Not Improving?
1. Monitor Redis with `redis-cli`: `MONITOR`
2. Check cache hit rates
3. Verify appropriate cache duration is set

## Best Practices

1. **Use Appropriate TTL**
   - Frequently changing data: 60-300 seconds
   - Relatively stable data: 300-600 seconds
   - Static data: 1800+ seconds

2. **Cache Key Patterns**
   - Use consistent prefixes (e.g., `product:*`, `user:*`)
   - Include entity identifiers for targeted invalidation

3. **Avoid Caching**
   - Sensitive user data
   - Real-time changing data
   - Large objects (consider pagination)

4. **Monitor Cache**
   - Track cache hit rates
   - Monitor Redis memory usage
   - Set up alerts for Redis unavailability

## Future Enhancements

- [ ] Add cache hit/miss metrics
- [ ] Implement cache warming strategies
- [ ] Add Redis Cluster support
- [ ] Implement cache key versioning
- [ ] Add per-user caching support

## References

- [Redis Documentation](https://redis.io/documentation)
- [StackExchange.Redis](https://github.com/StackExchange/StackExchange.Redis)
- [Microsoft Caching Documentation](https://docs.microsoft.com/en-us/aspnet/core/performance/caching/distributed)
