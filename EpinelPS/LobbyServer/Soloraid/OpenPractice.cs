using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Soloraid;

[PacketPath("/soloraid/practice/open")]
public class OpenPractice : LobbyMsgHandler
{
    protected override async Task HandleAsync()
    {
        // ReqOpenSoloRaidPractice Fields:
        //  int RaidLevel
        //  SoloRaidDifficultyType DifficultyType
        var req = await ReadData<ReqOpenSoloRaidPractice>();
        User user = GetUser();
        ResOpenSoloRaidPractice response = new()
        {
            PeriodResult = SoloRaidPeriodResult.Success,
        };

        try
        {
            SoloRaidHelper.OpenSoloRaid(user, 0, req.RaidLevel, type: SoloRaidType.Practice);
        }
        catch (Exception ex)
        {
            Logging.WriteLine($"OpenSoloRaid error: {ex.Message}", LogType.Error);
        }

        JsonDb.Save();
        await WriteDataAsync(response);
    }
}