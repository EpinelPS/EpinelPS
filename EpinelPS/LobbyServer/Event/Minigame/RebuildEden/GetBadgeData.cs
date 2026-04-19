using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Event.Minigame.RebuildEden;

[PacketPath("/event/minigame/rebuildeden/getbadge")]
public class GetBadgeData : LobbyMsgHandler
{
    protected override async Task HandleAsync()
    {
        ReqGetRebuildEdenBadge req = await ReadData<ReqGetRebuildEdenBadge>();
        User user = GetUser();

        ResGetRebuildEdenBadge response = new();

        // TODO implement

        await WriteDataAsync(response);
    }
}
