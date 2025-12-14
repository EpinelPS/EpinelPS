using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Soloraid;

[PacketPath("/soloraid/trial/getlevel")]
public class GetLevelTrial : LobbyMsgHandler
{
    protected override async Task HandleAsync()
    {
        // int RaidLevel
        var req = await ReadData<ReqGetLevelTrialSoloRaid>();
        User user = GetUser();
        ResGetLevelTrialSoloRaid response = new();

        try
        {
            SoloRaidHelper.GetLevelTrialInfo(user, ref response, req.RaidLevel);
        }
        catch (Exception ex)
        {
            Logging.WriteLine($"GetLevelTrial Error: {ex.Message}", LogType.Error);
        }

        await WriteDataAsync(response);
    }
}