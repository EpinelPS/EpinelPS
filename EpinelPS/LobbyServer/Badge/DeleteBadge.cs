using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Badge
{
    [PacketPath("/badge/delete")]
    public class DeleteBadge : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqDeleteBadge>();
            var user = GetUser();

            var response = new ResDeleteBadge();

            foreach (var badgeId in req.BadgeSeqList)
            {
                user.Badges.RemoveAll(x => x.Seq == badgeId);
            }

            JsonDb.Save();

            await WriteDataAsync(response);
        }
    }
}
