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

        public void Dispose()
        {

        }

        public void Log(LogMessageReceivedEventArgs logEvent)
        {
            var fg = Console.ForegroundColor;
            Console.ForegroundColor = GetColorForMsg(logEvent);

            var msg = logEvent.Message;
            if (msg.StartsWith("["))
            {
                msg = msg.Substring(msg.IndexOf("]") + 2);
            }

            // ignore telemtry server errors
            if (!msg.StartsWith("POST /v2/dr/getsid: \"404 Not Found\""))
            {
                Console.WriteLine(msg);
            }

            Console.ForegroundColor = fg;
        }

        private ConsoleColor GetColorForMsg(LogMessageReceivedEventArgs logEvent)
        {
            if (logEvent.Message.Contains("404 Not Found"))
                return ConsoleColor.Red;
            else if (logEvent.Message.Contains("200 OK"))
                return ConsoleColor.DarkGreen;
            switch (logEvent.MessageType)
            {
                case LogLevel.None:
                    return ConsoleColor.White;
                case LogLevel.Trace:
                    return ConsoleColor.Gray;
                case LogLevel.Debug:
                    return ConsoleColor.Gray;
                case LogLevel.Info:
                    return ConsoleColor.Gray;
                case LogLevel.Warning:
                    return ConsoleColor.Yellow;
                case LogLevel.Error:
                    return ConsoleColor.Red;
                case LogLevel.Fatal:
                    return ConsoleColor.Red;
                default:
                    return ConsoleColor.White;
            }
        }
    }
}
