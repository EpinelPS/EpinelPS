using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.LobbyUser
{
    [PacketPath("/lobby/usertitle/acquire")]
    public class AquireUserTitle : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqAcquireUserTitle>();
            var user = GetUser();

            var response = new ResAcquireUserTitle();
            
            // TODO

            await WriteDataAsync(response);
        }
    }
}
