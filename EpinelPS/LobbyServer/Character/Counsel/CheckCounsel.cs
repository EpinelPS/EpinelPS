using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Character.Counsel;

[GameRequest("/character/counsel/check")]
public class CheckCounsel : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqCounseledBefore req = await ReadData<ReqCounseledBefore>();
        User user = GetUser();

        ResCounseledBefore response = new()
        {
            IsCounseledBefore = CounselProgressHelper.HasCounseledBefore(user, req.CounselTid)
        };

        await WriteDataAsync(response);
    }
}
