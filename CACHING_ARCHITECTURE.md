# Caching Architecture & Flow Diagram

## System Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                         Client / API Gateway                      │
└────────────────────────┬────────────────────────────────────────┘
						 │
		┌────────────────┼────────────────┐
		│                │                │
	┌───▼────┐       ┌───▼────┐      ┌───▼────┐
	│  User  │       │Product │      │Category│
	│Service │       │Service │      │Service │
	│:7000   │       │:7001   │      │:7002   │
	└───┬────┘       └───┬────┘      └───┬────┘
		│                │                │
		│ ┌──────────────┼──────────────┐ │
		│ │              │              │ │
		└─┼──────────────┼──────────────┼─┘
		  │              │              │
	┌─────▼──────────────▼──────────────▼─────┐
	│                                          │
	│     Caching Action Filter                │
	│  (CachingActionFilter.cs)                │
	│                                          │
	│  - Intercepts all requests               │
	│  - Checks [Cacheable] attribute         │
	│  - Checks [InvalidateCache] attribute   │
	│  - Manages cache hit/miss logic         │
	│                                          │
	└─────┬────────────────────────────────┬──┘
		  │                                │
		  │ Cache Hit (Return Cached)      │
		  │ Cache Miss (Fetch from DB)     │
		  │                                │
	┌─────▼────────────────────────────────▼──┐
	│                                          │
	│         ICacheService                    │
	│      (CacheService.cs)                   │
	│                                          │
	│  - GetAsync<T>(key)                     │
	│  - SetAsync<T>(key, value, expiry)      │
	│  - RemoveAsync(key)                     │
	│                                          │
	└─────┬────────────────────────────────┬──┘
		  │                                │
		  │     IDistributedCache           │
		  │   (StackExchangeRedis)          │
		  │                                │
	┌─────▼────────────────────────────────▼──┐
	│                                          │
	│        Redis Cache Server                │
	│     localhost:6379 (Development)         │
	│     redis:6379 (Docker)                  │
	│                                          │
	│  Stores:                                 │
	│  - Serialized objects (JSON)            │
	│  - TTL (Time to Live)                   │
	│  - Instance: MyKart                     │
	│                                          │
	└──────────────────────────────────────────┘
		  │
		  └──► Database (Only on cache miss)
			   SQL Server
```

## Request Flow - GET Request (Cacheable)

```
User Request: GET /api/product
	   │
	   ▼
[CachingActionFilter] Intercepts
	   │
	   ├─ Has [Cacheable] attribute? YES
	   │
	   ▼
Generate Cache Key: "mykart:product:getallproducts"
	   │
	   ▼
Check Redis Cache
	   │
   ┌───┴───┐
   │       │
CACHE HIT │ CACHE MISS
   │      │
   ▼      ▼
Return │ Execute Controller Action
Cached │ (Fetch from Database)
Value  │
   │    ▼
   │ SetAsync to Redis
   │ (TTL: 300 seconds)
   │
   └─┬──┘
	 │
	 ▼
Return Response to Client
```

## Request Flow - Write Request (Cache Invalidation)

```
User Request: POST /api/product
	   │
	   ▼
[CachingActionFilter] Intercepts
	   │
	   ├─ Has [InvalidateCache] attribute? YES
	   │
	   ▼
Execute Controller Action
(Create/Update/Delete)
	   │
	   ├─ Success? YES
	   │
	   ▼
Extract Cache Patterns: ["product:*"]
	   │
	   ▼
Remove from Redis:
  - "mykart:product:getallproducts"
  - "mykart:product:getproductbyid:*"
  - All matching pattern keys
	   │
	   ▼
Return Response to Client
```

## Attribute Usage Overview

```
┌─────────────────────────────────────────────────────────────┐
│              ENDPOINT CACHING STRATEGY                       │
├─────────────────────────────────────────────────────────────┤
│                                                              │
│  GET /api/products                                           │
│  ├─ [Cacheable(300)] ─────────┐                            │
│  │                             ▼                             │
│  │                     Cache for 5 minutes                   │
│  │                     Return cached value                   │
│  │                     on subsequent hits                    │
│  │                                                           │
│  │                                                           │
│  POST /api/products                                          │
│  ├─ [InvalidateCache("product:*")] ────────┐               │
│  │                                          ▼                │
│  │                              Clear all product cache      │
│  │                              keys after insert            │
│  │                                                           │
│  │                                                           │
│  PUT /api/products/{id}                                      │
│  ├─ [InvalidateCache("product:*")] ────────┐               │
│  │                                          ▼                │
│  │                              Clear all product cache      │
│  │                              keys after update            │
│  │                                                           │
│  │                                                           │
│  DELETE /api/products/{id}                                   │
│  ├─ [InvalidateCache("product:*")] ────────┐               │
│  │                                          ▼                │
│  │                              Clear all product cache      │
│  │                              keys after delete            │
│                                                              │
└─────────────────────────────────────────────────────────────┘
```

## Cache Key Hierarchy

```
Redis Cache Keys Structure:
│
├── mykart:user:*
│   ├── mykart:user:getallusersdetails
│   ├── mykart:user:getuserbyid:john@example.com
│   └── mykart:user:getuserbyid:jane@example.com
│
├── mykart:product:*
│   ├── mykart:product:getallproducts
│   ├── mykart:product:getproductbyid:p123
│   ├── mykart:product:getproductbyid:p456
│   ├── mykart:product:getprice:p123
│   └── mykart:product:getprice:p456
│
├── mykart:category:*
│   ├── mykart:category:getallcategoriesdetails
│   ├── mykart:category:getcategorybyid:1
│   └── mykart:category:getcategorybyid:2
│
└── mykart:purchase:*
	├── mykart:purchase:getallproducts
	└── mykart:purchase:getallproducts:{userId}
```

## Service-to-Cache Mapping

```
┌──────────────────────────────────────────────────────────┐
│                SERVICE CACHE PATTERNS                     │
├──────────────────────────────────────────────────────────┤
│                                                          │
│  UserMicroservices                                       │
│  ├─ GET Endpoints: [Cacheable(300)]                     │
│  ├─ POST/PUT/DELETE: [InvalidateCache("user:*")]        │
│  └─ Cache Pattern: user:*                               │
│                                                          │
│  ProductMicroservices                                    │
│  ├─ GET Endpoints: [Cacheable(300)]                     │
│  ├─ POST/PUT/DELETE: [InvalidateCache("product:*")]     │
│  ├─ GET Price: [Cacheable(300)]                         │
│  ├─ PUT Quantity: [InvalidateCache("product:*")]        │
│  └─ Cache Pattern: product:*                            │
│                                                          │
│  CategoryMicroservices                                   │
│  ├─ GET Endpoints: [Cacheable(300)]                     │
│  ├─ POST/PUT/DELETE: [InvalidateCache("category:*")]    │
│  └─ Cache Pattern: category:*                           │
│                                                          │
│  PurchaseMicroservices                                   │
│  ├─ GET Endpoints: [Cacheable(300)]                     │
│  ├─ POST/PUT/DELETE: [InvalidateCache("purchase:*")]    │
│  ├─ PurchaseProduct: [InvalidateCache("purchase:*",     │
│  │                                    "product:*")]      │
│  └─ Cache Pattern: purchase:*, product:*                │
│                                                          │
└──────────────────────────────────────────────────────────┘
```

## Dependency Injection Flow

```
Program.cs Configuration:
│
├─ builder.Services.AddRedisCache(configuration)
│  │
│  └─► IServiceCollection.AddStackExchangeRedisCache()
│      │
│      └─► Redis Connection: "localhost:6379"
│          Instance Name: "MyKart"
│
├─ builder.Services.AddSingleton<ICacheService, CacheService>()
│  │
│  └─► Constructor: CacheService(IDistributedCache cache)
│
└─ builder.Services.AddScoped<CachingActionFilter>()
   │
   └─► Constructor: CachingActionFilter(ICacheService, IDistributedCache)
	   │
	   └─► Injected into all controller actions
```

## Cache Lifecycle

```
┌─────────────────────────────────────────────────────────┐
│                CACHE LIFECYCLE                          │
├─────────────────────────────────────────────────────────┤
│                                                          │
│  1. CREATION                                             │
│     └─ SetAsync(key, value, TTL: 300 seconds)           │
│                                                          │
│  2. STORAGE                                              │
│     └─ Stored in Redis as JSON string                   │
│        TTL countdown starts                             │
│                                                          │
│  3. RETRIEVAL                                            │
│     └─ GetAsync(key)                                    │
│        Deserialized back to original type               │
│                                                          │
│  4. EXPIRATION (Option A)                                │
│     └─ TTL expires (300 seconds)                        │
│        Key automatically deleted by Redis               │
│        Next request: Cache miss → Fetch from DB         │
│                                                          │
│  5. INVALIDATION (Option B)                              │
│     └─ Write operation triggers [InvalidateCache]       │
│        RemoveAsync(key) called immediately              │
│        Next request: Cache miss → Fetch fresh data      │
│                                                          │
│  6. DELETION (Manual)                                    │
│     └─ RemoveAsync(key) explicit call                   │
│        Can be done programmatically if needed           │
│                                                          │
└─────────────────────────────────────────────────────────┘
```

## Performance Impact

```
Response Time Comparison:
│
│ Without Cache:
│ Request → Database Query → Response
│ ────────────────────────────────────
│ Time: 250ms (Database dependent)
│
│ With Cache (Hit):
│ Request → Redis Lookup → Response
│ ────────────────────────────────────
│ Time: 5ms (Network + Deserialization)
│
│ Improvement: ~98% faster
│
│
Database Load Comparison:
│
│ Without Cache:
│ 1000 concurrent users
│ = 1000 database queries/sec
│ = High server load
│
│ With Cache:
│ 1000 concurrent users
│ = ~95% cache hits
│ = 50 database queries/sec
│ = ~95% reduction in load
```

## Configuration Locations

```
Configuration:
│
├─ appsettings.json (Development)
│  └─ "ConnectionStrings": { "Redis": "localhost:6379" }
│
├─ appsettings.Docker.json (Container)
│  └─ "ConnectionStrings": { "Redis": "redis:6379" }
│
├─ appsettings.Development.json (Development specific)
│
└─ Environment Variables (Production)
   └─ Set via Docker/Container configuration
```

## Error Handling

```
┌─────────────────────────────────────────────────────────┐
│              ERROR HANDLING STRATEGY                    │
├─────────────────────────────────────────────────────────┤
│                                                          │
│  Redis Unavailable?                                      │
│  └─ CachingActionFilter catches exception               │
│     └─ Silently falls back to database query            │
│        └─ Application continues normally               │
│           └─ Response may be slower but still valid     │
│                                                          │
│  Cache Invalidation Fails?                              │
│  └─ Exception caught and logged                         │
│     └─ Doesn't prevent response to user                 │
│        └─ Stale cache may be served briefly             │
│           └─ TTL ensures eventual consistency           │
│                                                          │
│  Serialization Error?                                    │
│  └─ Returns null (cache miss)                           │
│     └─ Falls back to database query                     │
│        └─ Ensures data integrity                        │
│                                                          │
└─────────────────────────────────────────────────────────┘
```
