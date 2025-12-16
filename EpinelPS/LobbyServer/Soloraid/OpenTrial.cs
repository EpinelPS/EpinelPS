using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Soloraid;

[PacketPath("/soloraid/trial/open")]
public class OpenTrial : LobbyMsgHandler
{
    protected override async Task HandleAsync()
    {
        // { "raidLevel": 8 }
        var req = await ReadData<ReqOpenSoloRaidTrial>();
        User user = GetUser();
        ResOpenSoloRaidTrial response = new()
        {
            PeriodResult = SoloRaidPeriodResult.Success,
            RaidOpenCount = 1,
        };

        try
        {
            response.RaidOpenCount = SoloRaidHelper.OpenSoloRaid(user, 0, req.RaidLevel, type: SoloRaidType.Trial);
        }
        catch (Exception ex)
        {
            Logging.WriteLine($"OpenSoloRaid error: {ex.Message}", LogType.Error);
        }

        JsonDb.Save();
        await WriteDataAsync(response);
    }
}