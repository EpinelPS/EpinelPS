using nksrv.Utils;

namespace nksrv.LobbyServer.Msgs.Arena
{
    [PacketPath("/arena/getbaninfo")]
    public class GetArenaBanInfo : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqGetArenaBanInfo>();

            var response = new ResGetArenaBanInfo();
            // TODO
            await WriteDataAsync(response);
        }
    }
}
