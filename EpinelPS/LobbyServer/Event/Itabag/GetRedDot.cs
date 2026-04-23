using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Event.Itabag;

[PacketPath("/Event/Itabag/RedDotData")]
public class GetRedDot : LobbyMsgHandler
{
    protected override async Task HandleAsync()
    {
        ReqItabagRedDotData req = await ReadData<ReqItabagRedDotData>();
        User user = GetUser();

        ReqItabagRedDotData res = new();

        // TODO
        await WriteDataAsync(res);
    }
}
