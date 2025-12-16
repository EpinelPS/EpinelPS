using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Soloraid;

[PacketPath("/soloraid/open")]
public class Open : LobbyMsgHandler
{
    protected override async Task HandleAsync()
    {
        // { "raidId": 1000030, "raidLevel": 1 }
        var req = await ReadData<ReqOpenSoloRaid>();
        User user = GetUser();
        ResOpenSoloRaid response = new()
        {
            PeriodResult = SoloRaidPeriodResult.Success,
        };

        try
        {
            int openCount = SoloRaidHelper.OpenSoloRaid(user, req.RaidId, req.RaidLevel);
            response.RaidOpenCount = openCount;
        }
        catch (Exception ex)
        {
            Logging.WriteLine($"OpenSoloRaid error: {ex.Message}", LogType.Error);
        }

        JsonDb.Save();
        await WriteDataAsync(response);
    }
}