using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Soloraid;

[GameRequest("/soloraid/close")]
public class Close : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        // int RaidId, int RaidLevel
        var req = await ReadData<ReqCloseSoloRaid>();
        var user = GetUser();
        ResCloseSoloRaid response = new()
        {
            PeriodResult = SoloRaidPeriodResult.Success
        };

        try
        {
            SoloRaidHelper.CloseSoloRaid(user, req.RaidId, req.RaidLevel, SoloRaidType.Normal);
        }
        catch (Exception ex)
        {
            Logging.WriteLine($"CloseSoloRaid Error: {ex.Message}", LogType.Error);
        }

        await WriteDataAsync(response);
    }
}