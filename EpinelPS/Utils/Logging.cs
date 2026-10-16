using log4net;
using System.ComponentModel.DataAnnotations;

namespace EpinelPS.Utils;

public static class Logging
{
    private static LogType LogLevel = LogType.Info;
    private static readonly ILog log = LogManager.GetLogger(typeof(Logging));
    private static readonly object _consoleLock = new();

    public static bool IsDebugEnabled => LogLevel <= LogType.Debug;

    public static void SetOutputLevel(LogType level)
    {
        LogLevel = level;
    }
    public static void Warn(string msg)
    {
        WriteLine(msg, LogType.Warning);
    }
    public static void WriteLine(string msg, LogType level = LogType.Info, bool toConsole = true)
    {
        if (level == LogType.Debug && !IsDebugEnabled && !log.IsDebugEnabled)
            return;
        switch (level)
        {
            case LogType.Debug:
                log.Debug(msg);
                break;
            case LogType.Info:
                log.Info(msg);
                break;
            case LogType.Warning:
                log.Warn(msg);
                break;
            case LogType.WarningAntiCheat:
                log.Warn(msg);
                break;
            case LogType.Error:
                log.Error(msg);
                break;
            default:
                log.Info(msg);
                break;
        }

        if (toConsole && LogLevel <= level)
        {
            lock (_consoleLock)
            {
                ConsoleColor originalFG = Console.ForegroundColor;
                Console.ForegroundColor = GetColorForLevel(level);
                Console.WriteLine(msg);
                Console.ForegroundColor = originalFG;
            }
        }
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