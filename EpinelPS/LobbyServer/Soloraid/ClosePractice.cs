using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Soloraid;

[PacketPath("/soloraid/practice/close")]
public class ClosePractice : LobbyMsgHandler
{
    protected override async Task HandleAsync()
    {
        // int RaidId, int RaidLevel
        var req = await ReadData<ReqCloseSoloRaidPractice>();
        var user = GetUser();
        ResCloseSoloRaidPractice response = new()
        {
            PeriodResult = SoloRaidPeriodResult.Success
        };

        try
        {
            SoloRaidHelper.CloseSoloRaid(user, req.RaidId, req.RaidLevel, SoloRaidType.Practice);
        }catch(Exception ex)
        {
            Logging.WriteLine($"CloseSoloRaidPractice Error: {ex.Message}", LogType.Error);
        }

        await WriteDataAsync(response);
    }
}