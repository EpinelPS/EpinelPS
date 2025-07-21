using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Badge
{
    [PacketPath("/badge/delete")]
    public class DeleteBadge : LobbyMsgHandler
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
}
