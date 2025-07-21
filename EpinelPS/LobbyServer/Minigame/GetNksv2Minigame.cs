using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Minigame
{
    [PacketPath("/minigame/nksv2/get")]
    public class GetNksv2Minigame : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqGetMiniGameNKSV2Data req = await ReadData<ReqGetMiniGameNKSV2Data>();

            ResGetMiniGameNKSV2Data response = new();
            // TODO
            await WriteDataAsync(response);
        }
    }
}
