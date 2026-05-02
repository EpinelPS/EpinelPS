using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Soloraid;

[GameRequest("/soloraid/getlevel")]
public class GetLevel : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        var req = await ReadData<ReqGetLevelSoloRaid>();
        User user = GetUser();
        ResGetLevelSoloRaid response = new();

        try
        {
            SoloRaidHelper.GetLevelInfo(user, ref response, req.RaidLevel);
        }
        catch (Exception ex)
        {
            Logging.WriteLine($"GetLevelSoloRaid Error: {ex.Message}", LogType.Error);
        }

        await WriteDataAsync(response);
    }
}