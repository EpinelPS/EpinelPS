using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Soloraid;

[GameRequest("/soloraid/fastbattle")]
public class FastBattle : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        var req = await ReadData<ReqFastBattleSoloRaid>();
        var user = GetUser();
        ResFastBattleSoloRaid response = new();

        try
        {
            SoloRaidHelper.FastBattle(user, ref response, req.RaidId, req.RaidLevel, req.ClearCount);
        }
        catch (Exception e)
        {
            Logging.WriteLine($"FastBattle Error {e.Message}");
        }

        await WriteDataAsync(response);
    }
}