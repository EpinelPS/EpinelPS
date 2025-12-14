using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Soloraid;

[PacketPath("/soloraid/get")]
public class GetInfo : LobbyMsgHandler
{
    protected override async Task HandleAsync()
    {
        await ReadData<ReqGetSoloRaidInfo>();
        User user = GetUser();

        // ResGetSoloRaidInfo Fields
        //  NetUserSoloRaidInfo Info
        //  SoloRaidPeriodResult PeriodResult
        //  SoloRaidBanResult BanResult
        ResGetSoloRaidInfo response = new()
        {
            Info = new NetUserSoloRaidInfo(),
            PeriodResult = SoloRaidPeriodResult.Success,
            BanResult = SoloRaidBanResult.Success
        };

        try
        {
            response.Info = SoloRaidHelper.GetUserSoloRaidInfo(user);
        }
        catch (Exception ex)
        {
            Logging.WriteLine($"GetSoloRaidInfo error: {ex.Message}", LogType.Error);
        }

        await WriteDataAsync(response);
    }
}