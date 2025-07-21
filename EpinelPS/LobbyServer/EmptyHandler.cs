using Google.Protobuf.WellKnownTypes;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer
{
    public class EmptyHandler : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqGetNow req = await ReadData<ReqGetNow>();
            ResGetNow response = new();

            await WriteDataAsync(response);
        }
    }
}
