using EpinelPS.Database;

namespace EpinelPS.Utils
{
    public class Logging
    {
        public static void WriteLine(string msg, LogType level = LogType.Info)
        {
            var originalFG = Console.ForegroundColor;
            Console.ForegroundColor = GetColorForLevel(level);

            // todo write to some file


            if (JsonDb.Instance.LogLevel <= level)
                Console.WriteLine(msg);

            Console.ForegroundColor = originalFG;

        }

        private static ConsoleColor GetColorForLevel(LogType level)
        {
            switch (level)
            {
                case LogType.Debug: return ConsoleColor.DarkGray;
                case LogType.Info: return ConsoleColor.Gray;
                case LogType.InfoSuccess: return ConsoleColor.Green;
                case LogType.Warning: return ConsoleColor.Yellow;
                case LogType.WarningAntiCheat: return ConsoleColor.DarkMagenta;
                case LogType.Error: return ConsoleColor.Red;
                default: return ConsoleColor.White;
            }
        }
    }

    public enum LogType
    {
        Debug,
        InfoSuccess,
        Info,
        Warning,
        WarningAntiCheat,
        Error
    }
}