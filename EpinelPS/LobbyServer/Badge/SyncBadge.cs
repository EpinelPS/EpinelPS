namespace EpinelPS.LobbyServer.Badge;

[GameRequest("/badge/sync")]
public class SyncBadge : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqSyncBadge req = await ReadData<ReqSyncBadge>();
        User user = GetUser();

        ResSyncBadge response = new();

        foreach (BadgeModel item in user.Badges)
        {
            response.BadgeList.Add(item.ToNet());
        }

        await WriteDataAsync(response);
    }
}
