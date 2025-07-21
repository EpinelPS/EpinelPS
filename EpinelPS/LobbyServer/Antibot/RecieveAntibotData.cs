using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Antibot
{
    [PacketPath("/antibot/recvdata")]
    public class RecieveAntibotData : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqAntibotRecvData req = await ReadData<ReqAntibotRecvData>();

            // I don't really care about reimplementing the server side anticheat, so return

            ResAntibotRecvData response = new();

            await WriteDataAsync(response);
        }
    }
}
