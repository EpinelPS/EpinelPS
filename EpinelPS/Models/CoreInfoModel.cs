using EpinelPS.Utils;

namespace EpinelPS.Models;

/// <summary>
/// NOTE: OBSOLETE: Add new entries to GameContext
/// </summary>
public class CoreInfo
{
    // TODO: Move to GameContext
    public RankData RankDatas { get; set; } = new();

    // General configuration will stay here
    public byte[] LauncherTokenKey { get; set; } = [];
    public int DbVersion { get; set; } = 5;
    public byte[] EncryptionTokenKey { get; set; } = [];
    public LogType LogLevel { get; set; } = LogType.Debug;

    public int MaxInterceptionCount { get; set; } = 3;
    public int ResetHourUtcTime { get; set; } = 20;
}