using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Outpost
{
    [PacketPath("/outpost/memorial/getmemorylist")]
    public class MemorialGetMemoryList : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqGetMemoryList>();
            var user = GetUser();

            var response = new ResGetMemoryList();

            response.MemoryList.AddRange(user.Memorial);

            // TODO rewards

            await WriteDataAsync(response);
        }
    }
}
