namespace SharedLibrary.Common
{
    /// <summary>
    /// Constants for logging configuration and messages.
    /// </summary>
    public static class LoggingConstants
    {
        // Log message templates
        public const string RequestStarted = "HTTP {Method} {Path} started - IP: {IpAddress}, CorrelationId: {CorrelationId}";
        public const string RequestCompleted = "HTTP {Method} {Path} completed with status {StatusCode} in {ElapsedMs}ms - CorrelationId: {CorrelationId}";
        public const string RequestFailed = "HTTP {Method} {Path} failed with status {StatusCode} in {ElapsedMs}ms - CorrelationId: {CorrelationId}";
        public const string UnhandledException = "Unhandled exception occurred - CorrelationId: {CorrelationId}";
        public const string OperationStarted = "Operation '{OperationName}' started - CorrelationId: {CorrelationId}";
        public const string OperationCompleted = "Operation '{OperationName}' completed in {ElapsedMs}ms - CorrelationId: {CorrelationId}";
        public const string OperationFailed = "Operation '{OperationName}' failed after {ElapsedMs}ms - CorrelationId: {CorrelationId}";
        public const string DatabaseQueryStarted = "Database query '{QueryName}' started - CorrelationId: {CorrelationId}";
        public const string DatabaseQueryCompleted = "Database query '{QueryName}' completed in {ElapsedMs}ms - CorrelationId: {CorrelationId}";
        public const string DatabaseQueryFailed = "Database query '{QueryName}' failed - CorrelationId: {CorrelationId}";

        // Sensitive field names to mask
        public static readonly string[] SensitiveFields = new[]
        {
            "password",
            "token",
            "authorization",
            "apikey",
            "api_key",
            "secret",
            "bearer",
            "credit_card",
            "creditcard",
            "cvv",
            "ssn",
            "social_security",
            "pin",
            "private_key",
            "privatekey",
            "access_token",
            "refresh_token",
            "id_token",
            "client_secret",
            "signing_secret",
            "webhook_secret",
            "salt",
            "nonce",
            "encrypted",
            "email",
            "phone",
            "mobile",
            "firstname",
            "lastname",
            "middlename",
            "dateofbirth",
            "dob",
            "address"
        };

        // Correlation ID header name
        public const string CorrelationIdHeader = "X-Correlation-ID";
        public const string CorrelationIdProperty = "CorrelationId";

        // Log level defaults
        public const string DefaultLogLevel = "Information";
    }
}
