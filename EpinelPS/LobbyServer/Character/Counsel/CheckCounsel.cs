using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Character.Counsel;

[PacketPath("/character/counsel/check")]
public class CheckCounsel : LobbyMsgHandler
{
    protected override async Task HandleAsync()
    {
        ReqCounseledBefore req = await ReadData<ReqCounseledBefore>();

        ResCounseledBefore response = new();

        response.IsCounseledBefore = false;

        await WriteDataAsync(response);
    }
}