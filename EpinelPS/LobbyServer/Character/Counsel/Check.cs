using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Character.Counsel;

[GameRequest("/character/attractive/check")]
public class CheckCharacterCounsel : LobbyMessage
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
