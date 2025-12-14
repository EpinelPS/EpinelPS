using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Soloraid;

[PacketPath("/soloraid/close")]
public class Close : LobbyMsgHandler
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
        }catch(Exception ex)
        {
            Logging.WriteLine($"CloseSoloRaid Error: {ex.Message}", LogType.Error);
        }

        await WriteDataAsync(response);
    }
}