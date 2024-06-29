using Swan.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nksrv.Utils
{
    public class GreatLogger : ILogger
    {
        public LogLevel LogLevel => LogLevel.Info;
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
                LogLevel.None => ConsoleColor.White,
                LogLevel.Trace => ConsoleColor.Gray,
                LogLevel.Debug => ConsoleColor.Gray,
                LogLevel.Info => ConsoleColor.Gray,
                LogLevel.Warning => ConsoleColor.Yellow,
                LogLevel.Error => ConsoleColor.Red,
                LogLevel.Fatal => ConsoleColor.Red,
                _ => ConsoleColor.White,
            };
        }
        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
