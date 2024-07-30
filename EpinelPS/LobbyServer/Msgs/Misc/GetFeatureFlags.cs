using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Msgs.Misc
{
    [PacketPath("/featureflags/get")]
    public class GetFeatureFlags : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqGetFeatureFlag>();

            var r = new ResGetFeatureFlag();
            r.IsOpen = true;

            await WriteDataAsync(r);
        }
    }
}
