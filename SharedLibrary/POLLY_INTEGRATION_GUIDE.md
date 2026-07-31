# Polly Integration Guide for MyKart Microservices

## Overview

Polly is a .NET resilience and transient-fault-handling library that helps your microservices handle failures gracefully. Your solution already has `Microsoft.Extensions.Http.Polly` (v8.0.0) installed.

## Available Polly Patterns

### 1. **Retry Policy**
Automatically retries failed requests with exponential backoff.
```csharp
builder.Services.AddHttpClient("ServiceName")
	.AddRetryPolicy(retryCount: 3, initialDelayMilliseconds: 100);
```

### 2. **Circuit Breaker Policy**
Stops making requests when a service is failing, preventing cascading failures.
```csharp
builder.Services.AddHttpClient("ServiceName")
	.AddCircuitBreakerPolicy(failureThreshold: 5, breakDurationSeconds: 30);
```

### 3. **Timeout Policy**
Cancels requests that take too long.
```csharp
builder.Services.AddHttpClient("ServiceName")
	.AddTimeoutPolicy(timeoutSeconds: 10);
```

### 4. **Bulkhead Isolation Policy**
Limits concurrent requests to prevent resource exhaustion.
```csharp
builder.Services.AddHttpClient("ServiceName")
	.AddBulkheadPolicy(maxParallelization: 10, maxQueuingActions: 50);
```

### 5. **Combined Policies (Recommended)**
Combines Retry + Circuit Breaker + Timeout for optimal resilience.
```csharp
builder.Services.AddHttpClient("ServiceName")
	.AddPollyPolicies();
```

## Implementation Examples

### Example 1: API Gateway (Program.cs)

```csharp
using SharedLibrary.Common;

var builder = WebApplication.CreateBuilder(args);

// Add Polly policies to HTTP clients
builder.Services.AddHttpClient("CategoryService", httpClient =>
{
	httpClient.BaseAddress = new Uri("http://category-service:5124");
})
.AddPollyPolicies();

builder.Services.AddHttpClient("ProductService", httpClient =>
{
	httpClient.BaseAddress = new Uri("http://product-service:21464");
})
.AddPollyPolicies();

builder.Services.AddHttpClient("UserService", httpClient =>
{
	httpClient.BaseAddress = new Uri("http://user-service:35805");
})
.AddPollyPolicies();

builder.Services.AddHttpClient("PurchaseService", httpClient =>
{
	httpClient.BaseAddress = new Uri("http://purchase-service:47513");
})
.AddPollyPolicies();

var app = builder.Build();
// ... rest of configuration
```

### Example 2: Microservice with Named HttpClient

```csharp
// In PurchaseMicroservices/Program.cs
using SharedLibrary.Common;

var builder = WebApplication.CreateBuilder(args);

// Add HttpClient with Polly policies
builder.Services.AddHttpClient("PropagatingClient")
	.AddHttpMessageHandler<AuthorizationPropagationHandler>()
	.AddPollyPolicies();

// Or add general HttpClient with policies
builder.Services.AddHttpClient()
	.AddPollyPolicies();

var app = builder.Build();
// ... rest of configuration
```

### Example 3: Custom Policy Configuration

```csharp
// More aggressive retry strategy
builder.Services.AddHttpClient("AggressiveService")
	.AddRetryPolicy(retryCount: 5, initialDelayMilliseconds: 50);

// Stricter circuit breaker
builder.Services.AddHttpClient("CriticalService")
	.AddCircuitBreakerPolicy(failureThreshold: 3, breakDurationSeconds: 60);

// Shorter timeout for quick-fail scenarios
builder.Services.AddHttpClient("FastService")
	.AddTimeoutPolicy(timeoutSeconds: 5);

// Combined with bulkhead for high-traffic services
builder.Services.AddHttpClient("HighTrafficService")
	.AddPollyPolicies()
	.AddBulkheadPolicy(maxParallelization: 50);
```

## Policy Behavior

### Retry Policy
- **Retries:** 3 times by default
- **Backoff:** Exponential (100ms, 200ms, 400ms, 800ms...)
- **Handles:** 
  - HttpRequestException
  - OperationCanceledException
  - 408 Timeout
  - 429 Too Many Requests
  - 503 Service Unavailable

### Circuit Breaker Policy
- **Failure Threshold:** 5 consecutive failures
- **Break Duration:** 30 seconds
- **Ignores:** 400 (Bad Request), 404 (Not Found)
- **States:**
  - **Closed:** Normal operation, requests pass through
  - **Open:** Service failing, requests blocked
  - **Half-Open:** Testing if service recovered

### Timeout Policy
- **Default:** 10 seconds
- **Behavior:** Cancels request if it exceeds duration

### Bulkhead Policy
- **Max Parallelization:** 10 concurrent requests
- **Max Queuing:** 50 queued requests
- **Behavior:** Queues excess requests or rejects them

## Monitoring & Logging

The policies log to console by default. For production, integrate with your logging framework:

```csharp
// In your microservice
private readonly ILogger<YourService> _logger;

// Polly will use this logger if integrated
.AddPollyPolicies() // Uses console logging by default
```

## Best Practices

1. **Use Combined Policies:** `AddPollyPolicies()` is recommended for most scenarios
2. **Adjust Timeouts:** Match your service's typical response time + buffer
3. **Circuit Breaker First:** Implement circuit breaker before retries to prevent hammering
4. **Monitor Metrics:** Track retry counts, circuit breaker trips
5. **Test Failure Scenarios:** Simulate service failures to verify behavior
6. **Document SLAs:** Define acceptable retry counts and timeouts per service

## Configuration for MyKart Services

Based on your microservices architecture:

| Service | Retry | CB Threshold | Timeout | Bulkhead |
|---------|-------|--------------|---------|----------|
| Category | 3 | 5 | 10s | 10 |
| Product | 3 | 5 | 10s | 10 |
| User | 3 | 5 | 10s | 20 |
| Purchase | 3 | 5 | 15s | 25 |

## Troubleshooting

### Circuit Breaker Opened
- **Symptom:** All requests failing with "circuit breaker open"
- **Cause:** Service failed 5+ times
- **Solution:** Wait 30s or restart the service

### Timeout Errors
- **Symptom:** Requests timing out before receiving response
- **Cause:** Network latency or slow service
- **Solution:** Increase timeout, check service health

### Bulkhead Rejected
- **Symptom:** Some requests rejected
- **Cause:** More concurrent requests than limit
- **Solution:** Increase maxParallelization or implement queue management

## Files Added

- `SharedLibrary/Common/PollyExtensions.cs` - Extension methods for Polly configuration
- `SharedLibrary/Common/POLLY_USAGE_GUIDE.cs` - Usage documentation

## References

- [Polly Documentation](https://github.com/App-vNext/Polly)
- [Microsoft.Extensions.Http.Polly](https://www.nuget.org/packages/Microsoft.Extensions.Http.Polly/)
- [Implementing resilience patterns](https://docs.microsoft.com/en-us/dotnet/architecture/microservices/implement-resilient-applications/)
