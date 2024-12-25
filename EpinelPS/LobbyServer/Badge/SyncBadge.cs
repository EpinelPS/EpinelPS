using EpinelPS.Utils;
using Google.Protobuf;

namespace EpinelPS.LobbyServer.Badge
{
    [PacketPath("/badge/sync")]
    public class SyncBadge : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqSyncBadge>();
            var user = GetUser();

            var response = new ResSyncBadge();

            foreach (var item in user.Badges)
            {
                response.BadgeList.Add(item.ToNet());
            }

            await WriteDataAsync(response);
        }
    }
}
