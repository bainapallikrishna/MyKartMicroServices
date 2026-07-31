/// <summary>
/// This file demonstrates how to integrate Polly resilience policies into your API Gateway
/// 
/// Polly provides the following resilience patterns:
/// 1. Retry - Automatically retry failed requests with exponential backoff
/// 2. Circuit Breaker - Stop making requests when a service is failing
/// 3. Timeout - Cancel requests that take too long
/// 4. Bulkhead Isolation - Limit concurrent requests to prevent cascading failures
/// 5. Fallback - Provide default responses or alternative actions
/// 
/// Usage in Program.cs:
/// 
/// // Add HTTP clients with Polly policies to the dependency injection container
/// builder.Services.AddHttpClient<ICategoryService, CategoryService>()
///     .AddPollyPolicies();  // Combines Retry + Circuit Breaker + Timeout
/// 
/// builder.Services.AddHttpClient<IProductService, ProductService>()
///     .AddRetryPolicy(retryCount: 3, initialDelayMilliseconds: 100)
///     .AddCircuitBreakerPolicy(failureThreshold: 5, breakDurationSeconds: 30)
///     .AddTimeoutPolicy(timeoutSeconds: 15);
/// 
/// builder.Services.AddHttpClient<IUserService, UserService>()
///     .AddPollyPolicies()
///     .AddBulkheadPolicy(maxParallelization: 20);
/// 
/// /// </summary>
public class PollyConfigurationExample
{
    /*
    POLLY PATTERN EXPLANATIONS:

    1. RETRY POLICY
       - Automatically retries failed requests
       - Uses exponential backoff: 100ms, 200ms, 400ms, 800ms...
       - Handles: HttpRequestException, OperationCanceledException
       - Retries on: 408 (Timeout), 429 (Too Many Requests), 503 (Service Unavailable)
       - Best for: Transient failures, temporary network issues

    2. CIRCUIT BREAKER POLICY
       - Opens after N consecutive failures
       - Blocks requests for a duration (default 30s)
       - Reduces load on failing service
       - Automatically closes when service recovers
       - Best for: Cascading failures, preventing hammering failed services

    3. TIMEOUT POLICY
       - Cancels requests that exceed duration
       - Prevents hanging requests
       - Default: 10 seconds
       - Best for: Preventing resource exhaustion

    4. BULKHEAD ISOLATION
       - Limits concurrent requests
       - Prevents one service from consuming all resources
       - Can queue excess requests
       - Best for: Resource protection, fair resource allocation

    5. COMBINATION (AddPollyPolicies)
       - Combines Retry + Circuit Breaker + Timeout
       - Optimal resilience configuration for microservices
       - Recommended for production use
    */
}
