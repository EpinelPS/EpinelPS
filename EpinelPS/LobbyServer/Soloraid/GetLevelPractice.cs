using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Soloraid;

[PacketPath("/soloraid/practice/getlevel")]
public class GetLevelPractice : LobbyMsgHandler
{
    protected override async Task HandleAsync()
    {
        var req = await ReadData<ReqGetLevelPracticeSoloRaid>();
        var user = GetUser();
        ResGetLevelPracticeSoloRaid response = new();
        
        try
        {
            SoloRaidHelper.GetLevelPracticeInfo(user, ref response, req.RaidLevel);
        }
        catch (Exception ex)
        {
            Logging.WriteLine($"GetLevelPractice Error: {ex.Message}", LogType.Error);
        }
        
        await WriteDataAsync(response);
    }
}