using EpinelPS.Utils;

namespace EpinelPS.Models;
public class CoreInfo
{
    public int DbVersion = 5;
    public List<User> Users = [];

    public List<AccessToken> LauncherAccessTokens = [];
    public Dictionary<string, ulong> AdminAuthTokens = [];

    public byte[] LauncherTokenKey = [];
    public byte[] EncryptionTokenKey = [];
    public LogType LogLevel = LogType.Debug;

    public int MaxInterceptionCount = 3;
    public int ResetHourUtcTime = 20;
}