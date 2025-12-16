using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Soloraid;

[PacketPath("/soloraid/getlevel")]
public class GetLevel : LobbyMsgHandler
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