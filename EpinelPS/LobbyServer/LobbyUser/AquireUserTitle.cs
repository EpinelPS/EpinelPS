using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.LobbyUser
{
    [PacketPath("/lobby/usertitle/acquire")]
    public class AquireUserTitle : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqAcquireUserTitle req = await ReadData<ReqAcquireUserTitle>();
            User user = User;

            ResAcquireUserTitle response = new();
            
            // TODO

            await WriteDataAsync(response);
        }
    }
}
