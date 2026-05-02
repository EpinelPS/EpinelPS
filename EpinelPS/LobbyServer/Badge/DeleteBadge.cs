using EpinelPS.Database;

namespace EpinelPS.LobbyServer.Badge;

[GameRequest("/badge/delete")]
public class DeleteBadge : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqDeleteBadge req = await ReadData<ReqDeleteBadge>();
        User user = GetUser();

        ResDeleteBadge response = new();

        foreach (long badgeId in req.BadgeSeqList)
        {
            user.Badges.RemoveAll(x => x.Seq == badgeId);
        }

        JsonDb.Save();

        await WriteDataAsync(response);
    }
}
