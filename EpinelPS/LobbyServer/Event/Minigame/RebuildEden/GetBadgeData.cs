namespace EpinelPS.LobbyServer.Event.Minigame.RebuildEden;

[GameRequest("/event/minigame/rebuildeden/getbadge")]
public class GetBadgeData : LobbyMessage
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
