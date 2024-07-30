using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Msgs.Outpost
{
    [PacketPath("/outpost/memorial/getmemorylist")]
    public class MemorialGetMemoryList : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqGetMemoryList>();

            var response = new ResGetMemoryList();
            // TODO
            await WriteDataAsync(response);
        }
    }
}
