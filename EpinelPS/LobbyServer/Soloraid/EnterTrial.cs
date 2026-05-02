using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Soloraid;

[GameRequest("/soloraid/trial/enter")]
public class EnterTrial : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        var req = await ReadData<ReqEnterSoloRaidTrial>();

        Logging.WriteLine($"Entering solo raid {req.RaidId} at level {req.RaidLevel} for user {GetUser().ID} team {req.Team} members");

        ResEnterSoloRaidTrial response = new()
        {
            PeriodResult = SoloRaidPeriodResult.Success,
        };

        await WriteDataAsync(response);
    }
}