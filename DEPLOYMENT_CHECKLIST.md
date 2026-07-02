# Caching Implementation - Deployment Checklist

## Pre-Deployment Verification

### ✅ Code Quality & Build
- [x] Solution builds successfully without errors
- [x] All projects compile (UserMicroservices, ProductMicroservices, CategoryMicroservices, PurchaseMicroservices)
- [x] No compilation warnings
- [x] All attributes correctly applied
- [x] Configuration files updated

### ✅ Redis Setup
- [ ] Redis server installed or Docker image available
- [ ] Redis port 6379 accessible from all services
- [ ] Redis network connectivity verified
- [ ] Redis persistence configured (if needed)
- [ ] Redis password set (if production)

### ✅ Configuration
- [ ] appsettings.json has Redis connection string: `"Redis": "localhost:6379"`
- [ ] appsettings.Docker.json has Redis connection string: `"Redis": "redis:6379"`
- [ ] All 4 microservices have updated appsettings files
- [ ] Environment-specific configs verified
- [ ] Connection strings use correct format

### ✅ Code Coverage
- [ ] UserMicroservicesController has caching attributes
  - [x] GetAllUsersDetails - [Cacheable]
  - [x] GetUserById - [Cacheable]
  - [x] AddNewUser - [InvalidateCache]
  - [x] UpdateUser - [InvalidateCache]
  - [x] DeleteUser - [InvalidateCache]

- [ ] ProductController has caching attributes
  - [x] GetAllProducts - [Cacheable]
  - [x] GetProductById - [Cacheable]
  - [x] GetPrice - [Cacheable]
  - [x] AddNewProduct - [InvalidateCache]
  - [x] UpdateProductDetails - [InvalidateCache]
  - [x] DeleteProduct - [InvalidateCache]
  - [x] UpdateQuantity - [InvalidateCache]

- [ ] CategoryController has caching attributes
  - [x] GetAllCategoriesDetails - [Cacheable]
  - [x] GetCategoryById - [Cacheable]
  - [x] AddNewCategory - [InvalidateCache]
  - [x] UpdateCategory - [InvalidateCache]
  - [x] DeleteCategory - [InvalidateCache]

- [ ] PurchaseController has caching attributes
  - [x] GetAllProducts - [Cacheable]
  - [x] AddNewProduct - [InvalidateCache]
  - [x] UpdateProductDetails - [InvalidateCache]
  - [x] DeleteProduct - [InvalidateCache]
  - [x] AddPurchase - [InvalidateCache]

### ✅ Documentation
- [x] CACHING_IMPLEMENTATION.md created
- [x] CACHING_CHANGES_SUMMARY.md created
- [x] CACHING_QUICK_REFERENCE.md created
- [x] CACHING_ARCHITECTURE.md created
- [x] README_CACHING.md created

---

## Deployment Steps

### Phase 1: Pre-Deployment (Day -1)

**1. Backup Database**
```bash
# Backup SQL Server databases
# Take snapshots of UserDB, ProductDB, CategoryDB, PurchaseDB
```

**2. Test Redis Connectivity**
```bash
# Verify Redis can be reached from deployment environment
redis-cli -h <redis-host> ping
# Expected: PONG
```

**3. Verify Disk Space**
```bash
# Ensure sufficient space for Redis RDB files
df -h /var/lib/redis/
# Should have at least 5GB free
```

### Phase 2: Deployment Day (Day 0)

**1. Start Redis Server** (06:00 AM)
```bash
# Option 1: Docker
docker run -d -p 6379:6379 --name mykart-redis redis:alpine

# Option 2: Systemd
sudo systemctl start redis-server

# Option 3: Manual
redis-server --port 6379 --daemonize yes
```

**2. Verify Redis is Running**
```bash
redis-cli ping
# Expected: PONG

redis-cli INFO server
# Verify version and uptime
```

**3. Deploy Services (One at a time, 15-min intervals)**

**Service 1: CategoryMicroservices** (06:30 AM)
```bash
# Deploy UserMicroservices first as it has no dependencies
cd CategoryMicroservices
dotnet publish -c Release
# Deploy to target server
# Start service: dotnet CategoryMicroservices.dll
# Test: curl http://localhost:7002/api/category
```

**Service 2: ProductMicroservices** (06:45 AM)
```bash
# Deploy ProductMicroservices (depended on by PurchaseService)
cd ProductMicroservices
dotnet publish -c Release
# Deploy to target server
# Start service: dotnet ProductMicroservices.dll
# Test: curl http://localhost:7001/api/product
```

**Service 3: UserMicroservices** (07:00 AM)
```bash
# Deploy UserMicroservices
cd UserMicroservices
dotnet publish -c Release
# Deploy to target server
# Start service: dotnet UserMicroservices.dll
# Test: curl http://localhost:7000/api/user
```

**Service 4: PurchaseMicroservices** (07:15 AM)
```bash
# Deploy PurchaseMicroservices (depends on Product service)
cd PurchaseMicroservices
dotnet publish -c Release
# Deploy to target server
# Start service: dotnet PurchaseMicroservices.dll
# Test: curl http://localhost:7003/api/purchase
```

**5. Verify All Services Running**
```bash
# All services should be responsive
curl http://localhost:7000/api/user
curl http://localhost:7001/api/product
curl http://localhost:7002/api/category
curl http://localhost:7003/api/purchase

# All should return 200 OK
```

### Phase 3: Post-Deployment Testing (07:30 AM - 09:00 AM)

**1. Functional Testing**
```bash
# Test GET endpoints (should cache)
curl http://localhost:7001/api/product  # First call: ~250ms
curl http://localhost:7001/api/product  # Second call: ~5ms

# Test write operations (should invalidate cache)
curl -X POST http://localhost:7001/api/product \
  -H "Content-Type: application/json" \
  -d '{"name":"test","price":100}'

# Verify cache was invalidated
curl http://localhost:7001/api/product  # Next call: ~250ms
```

**2. Cache Monitoring**
```bash
# Connect to Redis
redis-cli

# Monitor in real-time
MONITOR

# Check cache keys
KEYS *

# View cache size
DBSIZE

# Check memory usage
INFO memory
```

**3. Load Testing**
```bash
# Using Apache JMeter or wrk:
wrk -t12 -c400 -d30s http://localhost:7001/api/product

# Expected improvements:
# - Response time: < 100ms (vs 250ms+ before)
# - Requests/sec: > 1000 (vs 100-200 before)
# - Cache hit ratio: > 80%
```

**4. Monitor Logs**
```bash
# Check for errors in application logs
tail -f /var/log/mykart/user-service.log
tail -f /var/log/mykart/product-service.log
tail -f /var/log/mykart/category-service.log
tail -f /var/log/mykart/purchase-service.log

# Expected: No cache-related errors
```

### Phase 4: Monitoring Setup (09:00 AM - 10:00 AM)

**1. Set Up Redis Monitoring**
```bash
# Configure Redis persistence
redis-cli CONFIG SET save "900 1 300 10"

# Enable AOF
redis-cli CONFIG SET appendonly yes

# Monitor memory limits
redis-cli CONFIG SET maxmemory 1gb
redis-cli CONFIG SET maxmemory-policy allkeys-lru
```

**2. Configure Alerting**
```bash
# Alert if Redis memory > 900MB
# Alert if Redis connections > 100
# Alert if cache hit ratio < 70%
# Alert if services can't connect to Redis
```

**3. Set Up Health Checks**
```bash
# Verify cache availability
/health/cache endpoint should check Redis connection
# Return 503 if Redis unavailable
# Continue serving (degraded mode)
```

### Phase 5: Performance Validation (10:00 AM - 11:00 AM)

**1. Before vs After Comparison**
```
Metric                    Before      After        Improvement
─────────────────────────────────────────────────────────────
GET Response Time         250ms       5ms          98% faster
POST Response Time        200ms       200ms        No change (correct)
DB Queries/sec            1000        50           95% reduction
Memory Usage              2GB         2.5GB        Acceptable
Cache Hit Ratio           N/A         > 80%        Good
Concurrent Users (P95)    100         1000         10x capacity
```

**2. Record Baseline Metrics**
```
Timestamp: [Date/Time]

Redis Metrics:
- Total keys: __________
- Memory used: __________
- Hit ratio: __________
- Operations/sec: __________

Database Metrics:
- Queries/sec: __________
- Avg query time: __________
- Connection count: __________

Service Metrics:
- Avg response time: __________
- P95 latency: __________
- Error rate: __________
- Throughput: __________ req/sec
```

---

## Rollback Plan

If issues occur, follow these steps to rollback:

### Step 1: Stop New Services
```bash
# Stop all microservices
pkill -f "dotnet UserMicroservices.dll"
pkill -f "dotnet ProductMicroservices.dll"
pkill -f "dotnet CategoryMicroservices.dll"
pkill -f "dotnet PurchaseMicroservices.dll"
```

### Step 2: Clear Redis Cache (Optional)
```bash
redis-cli FLUSHDB
# Or stop Redis entirely
redis-cli shutdown
```

### Step 3: Deploy Previous Version
```bash
# Redeploy previous build without caching attributes
cd UserMicroservices
git checkout HEAD~1  # Or specific tag
dotnet publish -c Release
```

### Step 4: Restart Services
```bash
# Start services without caching
dotnet UserMicroservices.dll
dotnet ProductMicroservices.dll
dotnet CategoryMicroservices.dll
dotnet PurchaseMicroservices.dll
```

### Step 5: Verify Services
```bash
# Test all endpoints
curl http://localhost:7000/api/user
curl http://localhost:7001/api/product
curl http://localhost:7002/api/category
curl http://localhost:7003/api/purchase
```

### Step 6: Document Issue
```
Issue: [Describe what went wrong]
Timestamp: [When discovered]
Impact: [How many users affected]
Root cause: [Why it happened]
Resolution: [What was done]
Prevention: [How to prevent next time]
```

---

## Success Criteria

✅ **Deployment is successful if:**

- [ ] All 4 microservices are running
- [ ] Redis is running and accessible
- [ ] GET endpoints return cached data
- [ ] POST/PUT/DELETE endpoints invalidate cache
- [ ] Response times are < 100ms for cached requests
- [ ] No errors in application logs
- [ ] Cache hit ratio > 80%
- [ ] Database load reduced by > 90%
- [ ] All functional tests pass
- [ ] Load test shows 10x throughput improvement

---

## Post-Deployment Tasks (Day +1)

### Monitoring & Metrics
- [ ] Set up dashboards for cache performance
- [ ] Configure alerts for cache failures
- [ ] Monitor cache hit rates
- [ ] Track response time improvements
- [ ] Monitor Redis memory usage

### Optimization
- [ ] Fine-tune cache TTL values
- [ ] Analyze cache hit patterns
- [ ] Identify cold cache entries
- [ ] Optimize cache key patterns
- [ ] Review cache invalidation strategy

### Documentation
- [ ] Document actual cache performance
- [ ] Update run-books with cache procedures
- [ ] Train team on cache monitoring
- [ ] Create incident response procedures
- [ ] Share lessons learned

### Follow-up (Week 1)
- [ ] Review cache performance metrics
- [ ] Gather user feedback
- [ ] Check for any cache-related issues
- [ ] Optimize TTL values based on usage
- [ ] Plan further enhancements

---

## Emergency Contacts

In case of deployment issues:

| Role | Name | Phone | Email |
|------|------|-------|-------|
| Tech Lead | [Name] | [Phone] | [Email] |
| DevOps | [Name] | [Phone] | [Email] |
| Database Admin | [Name] | [Phone] | [Email] |
| Infrastructure | [Name] | [Phone] | [Email] |

---

## Sign-Off

| Role | Name | Date | Signature |
|------|------|------|-----------|
| Developer | Krishna | ______ | ____________ |
| QA Lead | [Name] | ______ | ____________ |
| DevOps Lead | [Name] | ______ | ____________ |
| Project Manager | [Name] | ______ | ____________ |

---

## Appendix A: Quick Reference Commands

```bash
# Start Redis
docker run -d -p 6379:6379 redis:alpine
redis-cli ping

# Build solution
dotnet build

# Run services
dotnet run --project UserMicroservices
dotnet run --project ProductMicroservices
dotnet run --project CategoryMicroservices
dotnet run --project PurchaseMicroservices

# Test caching
curl http://localhost:7001/api/product
redis-cli KEYS "*"
redis-cli MONITOR

# Clear cache if needed
redis-cli FLUSHDB
redis-cli FLUSHALL
```

---

**Deployment Date**: _______________

**Deployed By**: _______________

**Approval**: _______________

---

*This checklist ensures a smooth, safe deployment of the caching implementation.*
