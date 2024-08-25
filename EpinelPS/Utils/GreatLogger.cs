using Swan.Logging;

namespace EpinelPS.Utils
{
    public class GreatLogger : Swan.Logging.ILogger
    {
        public Swan.Logging.LogLevel LogLevel => Swan.Logging.LogLevel.Info;
        static readonly object lockObject = new();
        public void Log(LogMessageReceivedEventArgs logEvent)
        {
            var msg = logEvent.Message;

            // strip out request id that embedio prints
            if (msg.StartsWith('['))
            {
                msg = msg[(msg.IndexOf("]") + 2)..];
            }

            // ignore telemtry server errors
            if (msg.StartsWith("POST /v2/dr/"))
            {
                return;
            }

            var newFG = GetColorForMsg(logEvent);

            lock (lockObject)
            {
                var oldFG = Console.ForegroundColor;
                Console.ForegroundColor = newFG;
                Console.WriteLine(msg);
                if (logEvent.Exception != null)
                    Console.WriteLine(logEvent.Exception);
                Console.ForegroundColor = oldFG;
            }

        }

        private static ConsoleColor GetColorForMsg(LogMessageReceivedEventArgs logEvent)
        {
            if (logEvent.Message.Contains("404 Not Found"))
                return ConsoleColor.Red;
            else if (logEvent.Message.Contains("200 OK"))
                return ConsoleColor.DarkGreen;
            return logEvent.MessageType switch
            {
                Swan.Logging.LogLevel.None => ConsoleColor.White,
                Swan.Logging.LogLevel.Trace => ConsoleColor.Gray,
                Swan.Logging.LogLevel.Debug => ConsoleColor.Gray,
                Swan.Logging.LogLevel.Info => ConsoleColor.Gray,
                Swan.Logging.LogLevel.Warning => ConsoleColor.Yellow,
                Swan.Logging.LogLevel.Error => ConsoleColor.Red,
                Swan.Logging.LogLevel.Fatal => ConsoleColor.Red,
                _ => ConsoleColor.White,
            };
        }
        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
