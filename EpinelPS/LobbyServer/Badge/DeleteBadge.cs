using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Badge
{
    [PacketPath("/badge/delete")]
    public class DeleteBadge : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqDeleteBadge>();

            var response = new ResDeleteBadge();

            await WriteDataAsync(response);
        }
    }
}
