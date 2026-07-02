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
            var logRecord = new
            {
                Timestamp = DateTime.UtcNow.ToString("O"),
                Level = logEntry.LogLevel.ToString(),
                Category = logEntry.Category,
                EventId = logEntry.EventId.Id,
                Message = logEntry.Formatter(logEntry.State, logEntry.Exception),
                Exception = logEntry.Exception?.ToString(),
                State = logEntry.State
            };

            var json = JsonSerializer.Serialize(logRecord, new JsonSerializerOptions { WriteIndented = false });
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
