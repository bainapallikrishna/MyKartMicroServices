using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using System.Net;

namespace SharedLibrary.Common;

/// <summary>
/// Extension methods for configuring Polly resilience policies
/// </summary>
public static class PollyExtensions
{
    /// <summary>
    /// Adds Polly HTTP policies (Retry, Circuit Breaker, Timeout) to the HTTP client
    /// </summary>
    public static IHttpClientBuilder AddPollyPolicies(this IHttpClientBuilder builder)
    {
        // Retry policy: Retry up to 3 times on transient failures
        var retryPolicy = Policy
            .Handle<HttpRequestException>()
            .Or<OperationCanceledException>()
            .OrResult<HttpResponseMessage>(r => 
                r.StatusCode == HttpStatusCode.RequestTimeout ||
                r.StatusCode == HttpStatusCode.TooManyRequests ||
                r.StatusCode == HttpStatusCode.ServiceUnavailable)
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: attempt => TimeSpan.FromMilliseconds(Math.Pow(2, attempt) * 100),
                onRetry: (outcome, timespan, retryCount, context) =>
                {
                    Console.WriteLine($"Retry attempt {retryCount} after {timespan.TotalMilliseconds}ms");
                });

        // Circuit Breaker policy: Break after 5 consecutive failures for 30 seconds
        var circuitBreakerPolicy = Policy
            .Handle<HttpRequestException>()
            .OrResult<HttpResponseMessage>(r => 
                !r.IsSuccessStatusCode &&
                r.StatusCode != HttpStatusCode.BadRequest &&
                r.StatusCode != HttpStatusCode.NotFound)
            .CircuitBreakerAsync(
                handledEventsAllowedBeforeBreaking: 5,
                durationOfBreak: TimeSpan.FromSeconds(30),
                onBreak: (outcome, timespan) =>
                {
                    Console.WriteLine($"Circuit breaker opened for {timespan.TotalSeconds}s");
                },
                onReset: () =>
                {
                    Console.WriteLine("Circuit breaker reset");
                });

        // Timeout policy: 10 seconds timeout
        var timeoutPolicy = Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromSeconds(10));

        // Combine all policies
        var combinedPolicy = Policy.WrapAsync(retryPolicy, circuitBreakerPolicy, timeoutPolicy);

        builder.AddPolicyHandler(combinedPolicy);

        return builder;
    }

    /// <summary>
    /// Adds a custom retry policy to the HTTP client
    /// </summary>
    public static IHttpClientBuilder AddRetryPolicy(
        this IHttpClientBuilder builder,
        int retryCount = 3,
        int initialDelayMilliseconds = 100)
    {
        var retryPolicy = Policy
            .Handle<HttpRequestException>()
            .Or<OperationCanceledException>()
            .OrResult<HttpResponseMessage>(r =>
                r.StatusCode == HttpStatusCode.RequestTimeout ||
                r.StatusCode == HttpStatusCode.TooManyRequests ||
                r.StatusCode == HttpStatusCode.ServiceUnavailable)
            .WaitAndRetryAsync(
                retryCount: retryCount,
                sleepDurationProvider: attempt => TimeSpan.FromMilliseconds(Math.Pow(2, attempt) * initialDelayMilliseconds),
                onRetry: (outcome, timespan, retryAttempt, context) =>
                {
                    Console.WriteLine($"Retry attempt {retryAttempt} after {timespan.TotalMilliseconds}ms");
                });

        builder.AddPolicyHandler(retryPolicy);

        return builder;
    }

    /// <summary>
    /// Adds a circuit breaker policy to the HTTP client
    /// </summary>
    public static IHttpClientBuilder AddCircuitBreakerPolicy(
        this IHttpClientBuilder builder,
        int failureThreshold = 5,
        int breakDurationSeconds = 30)
    {
        var circuitBreakerPolicy = Policy
            .Handle<HttpRequestException>()
            .OrResult<HttpResponseMessage>(r =>
                !r.IsSuccessStatusCode &&
                r.StatusCode != HttpStatusCode.BadRequest &&
                r.StatusCode != HttpStatusCode.NotFound)
            .CircuitBreakerAsync(
                handledEventsAllowedBeforeBreaking: failureThreshold,
                durationOfBreak: TimeSpan.FromSeconds(breakDurationSeconds),
                onBreak: (outcome, timespan) =>
                {
                    Console.WriteLine($"Circuit breaker opened for {timespan.TotalSeconds}s due to failures");
                },
                onReset: () =>
                {
                    Console.WriteLine("Circuit breaker reset - service is available again");
                });

        builder.AddPolicyHandler(circuitBreakerPolicy);

        return builder;
    }

    /// <summary>
    /// Adds a timeout policy to the HTTP client
    /// </summary>
    public static IHttpClientBuilder AddTimeoutPolicy(
        this IHttpClientBuilder builder,
        int timeoutSeconds = 10)
    {
        var timeoutPolicy = Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromSeconds(timeoutSeconds));

        builder.AddPolicyHandler(timeoutPolicy);

        return builder;
    }

    /// <summary>
    /// Adds bulkhead isolation policy to limit concurrent requests
    /// </summary>
    public static IHttpClientBuilder AddBulkheadPolicy(
        this IHttpClientBuilder builder,
        int maxParallelization = 10,
        int maxQueuingActions = 50)
    {
        var bulkheadPolicy = Policy.BulkheadAsync<HttpResponseMessage>(
            maxParallelization: maxParallelization,
            maxQueuingActions: maxQueuingActions,
            onBulkheadRejectedAsync: context =>
            {
                Console.WriteLine($"Bulkhead policy rejected request. Max parallelization: {maxParallelization}");
                return Task.CompletedTask;
            });

        builder.AddPolicyHandler(bulkheadPolicy);

        return builder;
    }
}
