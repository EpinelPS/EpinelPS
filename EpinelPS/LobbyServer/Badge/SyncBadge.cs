using EpinelPS.Utils;
using Google.Protobuf;

namespace EpinelPS.LobbyServer.Badge
{
    [PacketPath("/badge/sync")]
    public class SyncBadge : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqSyncBadge req = await ReadData<ReqSyncBadge>();
            Database.User user = GetUser();

            ResSyncBadge response = new();

            foreach (Database.Badge item in user.Badges)
            {
                response.BadgeList.Add(item.ToNet());
            }

            await WriteDataAsync(response);
        }
    }
}
