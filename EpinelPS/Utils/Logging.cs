using System.ComponentModel.DataAnnotations;
using EpinelPS.Database;

namespace EpinelPS.Utils
{
    public static class Logging
    {
        private static LogType LogLevel = LogType.Info;
        public static void SetOutputLevel(LogType level)
        {
            LogLevel = level;
        }
        public static void WriteLine(string msg, LogType level = LogType.Info)
        {
            var originalFG = Console.ForegroundColor;
            Console.ForegroundColor = GetColorForLevel(level);

            // todo write to some file


            if (LogLevel <= level)
                Console.WriteLine(msg);

            Console.ForegroundColor = originalFG;

        }

        private static ConsoleColor GetColorForLevel(LogType level)
        {
            switch (level)
            {
                case LogType.Debug: return ConsoleColor.DarkGray;
                case LogType.Info: return ConsoleColor.Gray;
                case LogType.Warning: return ConsoleColor.Yellow;
                case LogType.WarningAntiCheat: return ConsoleColor.DarkMagenta;
                case LogType.Error: return ConsoleColor.Red;
                default: return ConsoleColor.White;
            }
        }
    }

    public enum LogType
    {
        [Display(Name = "Debug")]
        Debug,
        [Display(Name = "Info")]
        Info,
        [Display(Name = "Warning")]
        Warning,
        [Display(Name = "Anticheat warnings")]
        WarningAntiCheat,
        [Display(Name = "Errors")]
        Error
    }
}