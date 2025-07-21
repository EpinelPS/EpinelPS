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
        public static void Warn(string msg)
        {
            WriteLine(msg, LogType.Warning);
        }
        public static void WriteLine(string msg, LogType level = LogType.Info)
        {
            ConsoleColor originalFG = Console.ForegroundColor;
            Console.ForegroundColor = GetColorForLevel(level);

            // todo write to some file


            if (LogLevel <= level)
                Console.WriteLine(msg);

            Console.ForegroundColor = originalFG;

        }

        private static ConsoleColor GetColorForLevel(LogType level)
        {
            return level switch
            {
                LogType.Debug => ConsoleColor.DarkGray,
                LogType.Info => ConsoleColor.Gray,
                LogType.Warning => ConsoleColor.Yellow,
                LogType.WarningAntiCheat => ConsoleColor.DarkMagenta,
                LogType.Error => ConsoleColor.Red,
                _ => ConsoleColor.White,
            };
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