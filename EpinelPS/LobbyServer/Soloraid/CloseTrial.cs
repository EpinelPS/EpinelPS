using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Soloraid;

[GameRequest("/soloraid/trial/close")]
public class CloseTrial : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        // int RaidId, int RaidLevel
        var req = await ReadData<ReqCloseSoloRaidTrial>();
        var user = GetUser();
        ResCloseSoloRaidTrial response = new()
        {
            PeriodResult = SoloRaidPeriodResult.Success
        };

        try
        {
            SoloRaidHelper.CloseSoloRaid(user, req.RaidId, req.RaidLevel, SoloRaidType.Trial);
        }
        catch (Exception ex)
        {
            Logging.WriteLine($"CloseSoloRaidTrial Error: {ex.Message}", LogType.Error);
        }

        await WriteDataAsync(response);
    }
}