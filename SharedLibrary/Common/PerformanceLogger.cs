using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;

namespace SharedLibrary.Common
{
    /// <summary>
    /// Utility class for logging operation performance metrics.
    /// </summary>
    public sealed class PerformanceLogger : IDisposable
    {
        private readonly ILogger _logger;
        private readonly string _operationName;
        private readonly string _correlationId;
        private Stopwatch _stopwatch;

        public PerformanceLogger(ILogger logger, string operationName, string correlationId = null)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _operationName = operationName ?? throw new ArgumentNullException(nameof(operationName));
            _correlationId = correlationId ?? "unknown";
            _stopwatch = Stopwatch.StartNew();

            _logger.LogInformation(
                LoggingConstants.OperationStarted,
                _operationName,
                _correlationId);
        }

        /// <summary>
        /// Logs successful operation completion with timing.
        /// </summary>
        public void LogSuccess()
        {
            _stopwatch.Stop();
            _logger.LogInformation(
                LoggingConstants.OperationCompleted,
                _operationName,
                _stopwatch.ElapsedMilliseconds,
                _correlationId);
        }

        /// <summary>
        /// Logs failed operation with timing and exception.
        /// </summary>
        public void LogFailure(Exception ex = null)
        {
            _stopwatch.Stop();
            if (ex != null)
            {
                _logger.LogError(ex,
                    LoggingConstants.OperationFailed,
                    _operationName,
                    _stopwatch.ElapsedMilliseconds,
                    _correlationId);
            }
            else
            {
                _logger.LogWarning(
                    LoggingConstants.OperationFailed,
                    _operationName,
                    _stopwatch.ElapsedMilliseconds,
                    _correlationId);
            }
        }

        /// <summary>
        /// Gets the elapsed time in milliseconds.
        /// </summary>
        public long ElapsedMilliseconds
        {
            get
            {
                if (_stopwatch.IsRunning)
                    return _stopwatch.ElapsedMilliseconds;
                return _stopwatch.ElapsedMilliseconds;
            }
        }

        public void Dispose()
        {
            _stopwatch = null;
        }
    }
}
