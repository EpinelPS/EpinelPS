using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Character.Counsel;

[PacketPath("/character/attractive/check")]
public class CheckCharacterCounsel : LobbyMsgHandler
{
    protected override async Task HandleAsync()
    {
        ReqCounseledBefore req = await ReadData<ReqCounseledBefore>();
        User user = GetUser();

        ResCounseledBefore response = new();

        // TODO: Validate response from real server and pull info from user info
        await WriteDataAsync(response);
    }
}
