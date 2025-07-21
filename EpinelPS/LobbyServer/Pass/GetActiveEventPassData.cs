using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Pass
{
    [PacketPath("/pass/event/getactive")]
    public class GetActiveEventPassData : LobbyMsgHandler
    {
		//broken game wont boot if not empty not sure how to implement this one
        protected override async Task HandleAsync()
        {
            ReqGetActiveEventPassData req = await ReadData<ReqGetActiveEventPassData>(); // no fields

            ResGetActiveEventPassData response = new(); // fields PassList = NetPassInfo



            await WriteDataAsync(response);
        }
    }
}
