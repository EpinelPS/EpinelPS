using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Pass
{
    [PacketPath("/pass/event/freereward/getactive")]
    public class GetActiveFreeRewardData : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqGetActiveFreeRewardPassData>();
            ResGetActiveFreeRewardPassData response = new();

            await WriteDataAsync(response);
        }
    }
}
