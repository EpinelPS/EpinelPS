using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Character.Counsel;

[PacketPath("/character/attractive/check")]
public class CheckCharacterCounsel : LobbyMsgHandler
{
    protected override async Task HandleAsync()
    {
        var req = await ReadData<ReqCounseledBefore>();
        var user = GetUser();

        ResCounseledBefore response = new();

        // TODO: Validate response from real server and pull info from user info
        await WriteDataAsync(response);
    }
}
