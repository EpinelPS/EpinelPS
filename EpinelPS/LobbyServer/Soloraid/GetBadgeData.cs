using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.SoloraId;

[PacketPath("/soloraidmuseum/get/reddotdata")]
public class GetBadgeData : LobbyMsgHandler
{
    protected override async Task HandleAsync()
    {
        ReqGetSoloRaidMuseumRedDotData req = await ReadData<ReqGetSoloRaidMuseumRedDotData>();

        ResGetSoloRaidMuseumRedDotData response = new();

        // TODO

        await WriteDataAsync(response);
    }
}
