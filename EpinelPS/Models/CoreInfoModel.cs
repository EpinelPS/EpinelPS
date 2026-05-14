using EpinelPS.Utils;

namespace EpinelPS.Models;

public class CoreInfo
{
    public int DbVersion { get; set; } = 5;
    public List<User> Users { get; set; } = [];

    public List<AccessToken> LauncherAccessTokens { get; set; } = [];
    public Dictionary<string, ulong> AdminAuthTokens { get; set; } = [];

    public byte[] LauncherTokenKey { get; set; } = [];
    public byte[] EncryptionTokenKey { get; set; } = [];
    public LogType LogLevel { get; set; } = LogType.Debug;

    public int MaxInterceptionCount { get; set; } = 3;
    public int ResetHourUtcTime { get; set; } = 20;
}