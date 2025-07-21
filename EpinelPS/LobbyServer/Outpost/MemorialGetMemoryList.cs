using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Outpost
{
    [PacketPath("/outpost/memorial/getmemorylist")]
    public class MemorialGetMemoryList : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqGetMemoryList req = await ReadData<ReqGetMemoryList>();
            Database.User user = GetUser();

            ResGetMemoryList response = new();

            response.MemoryList.AddRange(user.Memorial);

            // TODO rewards

            await WriteDataAsync(response);
        }
    }
}
