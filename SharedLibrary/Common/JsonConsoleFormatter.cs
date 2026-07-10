using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging.Console;
using System;
using System.IO;
using System.Text.Json;

namespace SharedLibrary.Common
{
    /// <summary>
    /// Custom JSON console formatter for structured logging output.
    /// </summary>
    public sealed class JsonConsoleFormatter : ConsoleFormatter, IDisposable
    {
        public JsonConsoleFormatter() : base(nameof(JsonConsoleFormatter))
        {
        }

        public override void Write<TState>(
            in LogEntry<TState> logEntry,
            IExternalScopeProvider scopeProvider,
            TextWriter textWriter)
        {
            // Convert State to a safe string representation to avoid reflecting into
            // platform-sensitive types (e.g. EndPoint) that can throw during serialization.
            string safeState = logEntry.State?.ToString();

            var logRecord = new
            {
                Timestamp = DateTime.UtcNow.ToString("O"),
                Level = logEntry.LogLevel.ToString(),
                Category = logEntry.Category,
                EventId = logEntry.EventId.Id,
                Message = logEntry.Formatter?.Invoke(logEntry.State, logEntry.Exception),
                Exception = logEntry.Exception?.ToString(),
                State = safeState
            };

            string json;
            try
            {
                json = JsonSerializer.Serialize(logRecord, new JsonSerializerOptions { WriteIndented = false });
            }
            catch (Exception)
            {
                // Fallback: avoid including State or other problematic members
                var fallback = new
                {
                    logRecord.Timestamp,
                    logRecord.Level,
                    logRecord.Category,
                    logRecord.EventId,
                    logRecord.Message,
                    logRecord.Exception,
                    State = safeState ?? string.Empty
                };

                json = JsonSerializer.Serialize(fallback, new JsonSerializerOptions { WriteIndented = false });
            }

            textWriter.WriteLine(json);
        }

        public void Dispose()
        {
        }
    }

    /// <summary>
    /// Options for JSON console formatter.
    /// </summary>
    public class JsonConsoleFormatterOptions : ConsoleFormatterOptions
    {
        public bool IncludeScopes { get; set; } = true;
        public bool UseUtcTimestamp { get; set; } = true;
        public bool IncludeEventId { get; set; } = true;
    }
}
