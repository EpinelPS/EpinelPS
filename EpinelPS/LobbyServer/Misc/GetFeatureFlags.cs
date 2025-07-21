using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Misc
{
    [PacketPath("/featureflags/get")]
    public class GetFeatureFlags : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqGetFeatureFlag req = await ReadData<ReqGetFeatureFlag>();

            ResGetFeatureFlag r = new()
            {
                IsOpen = true
            };

            await WriteDataAsync(r);
        }
    }
}
