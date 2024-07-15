using nksrv.Net;
using nksrv.Utils;

namespace nksrv.LobbyServer.Msgs.Antibot
{
    [PacketPath("/antibot/recvdata")]
    public class RecieveAntibotData : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqAntibotRecvData>();

            // I don't really care about reimplementing the server side anticheat, so return

            var response = new ResAntibotRecvData();

            await WriteDataAsync(response);
        }
    }
}
