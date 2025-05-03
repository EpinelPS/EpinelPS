using Google.Protobuf.WellKnownTypes;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer
{
    public class EmptyHandler : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqGetNow>();
            var response = new ResGetNow();

            await WriteDataAsync(response);
        }
    }
}
