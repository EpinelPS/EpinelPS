using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using System.Runtime.Versioning;

namespace EpinelPS.Utils
{
    [UnsupportedOSPlatform("browser")]
    [ProviderAlias("ColorConsole")]
    public sealed class ColorConsoleLoggerProvider : ILoggerProvider
    {
        private readonly IDisposable? _onChangeToken;
        private ColorConsoleLoggerConfiguration _currentConfig;
        private readonly ConcurrentDictionary<string, ColorConsoleLogger> _loggers =
            new(StringComparer.OrdinalIgnoreCase);

        public ColorConsoleLoggerProvider(
            IOptionsMonitor<ColorConsoleLoggerConfiguration> config)
        {
            _currentConfig = config.CurrentValue;
            _onChangeToken = config.OnChange(updatedConfig => _currentConfig = updatedConfig);
        }

        public ILogger CreateLogger(string categoryName) =>
            _loggers.GetOrAdd(categoryName, name => new ColorConsoleLogger(name, GetCurrentConfig));

        private ColorConsoleLoggerConfiguration GetCurrentConfig() => _currentConfig;

        public void Dispose()
        {
            _loggers.Clear();
            _onChangeToken?.Dispose();
        }
    }
    public sealed class ColorConsoleLoggerConfiguration
    {
        public int EventId { get; set; }

        public Dictionary<LogLevel, ConsoleColor> LogLevelToColorMap { get; set; } = new()
        {
            [LogLevel.Information] = ConsoleColor.Green
        };
    }
    public sealed class ColorConsoleLogger(
    string name,
    Func<ColorConsoleLoggerConfiguration> getCurrentConfig) : ILogger
    {
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => default!;

        public bool IsEnabled(LogLevel logLevel) =>
            getCurrentConfig().LogLevelToColorMap.ContainsKey(logLevel);

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }

            ColorConsoleLoggerConfiguration config = getCurrentConfig();
            if (config.EventId == 0 || config.EventId == eventId.Id)
            {
                ConsoleColor originalColor = Console.ForegroundColor;

                string msg = formatter(state, exception).Replace("Request reached the end of the middleware pipeline without being handled by application code. Request path: ", "");

                if (msg.StartsWith("Executing ObjectResult") || msg.StartsWith("Executed endpoint") || msg.StartsWith("Route matched with ") || msg.Contains("CatchAll") || msg.Contains("$batch") || msg.Contains("/dr/getsid"))
                    return;

                Console.ForegroundColor = config.LogLevelToColorMap[logLevel];
                Console.Write($"{msg}");
                if (exception != null)
                    Console.WriteLine(exception.ToString());

                Console.ForegroundColor = originalColor;
                Console.WriteLine();
            }
        }
    }
}
