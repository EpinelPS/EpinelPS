using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Soloraid;

[GameRequest("/soloraid/enter")]
public class Enter : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        var req = await ReadData<ReqEnterSoloRaid>();

        Logging.WriteLine($"Entering solo raid {req.RaidId} at level {req.RaidLevel} for user {GetUser().ID} team {req.Team} members");

        ResEnterSoloRaid response = new()
        {
            PeriodResult = SoloRaidPeriodResult.Success,
        };

        await WriteDataAsync(response);
    }
}