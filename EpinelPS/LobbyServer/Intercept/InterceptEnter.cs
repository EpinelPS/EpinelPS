using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Intercept
{
    [PacketPath("/intercept/enter")]
    public class EnterInterceptData : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqEnterIntercept>();

            var response = new ResEnterIntercept();

            await WriteDataAsync(response);
        }
    }
}
