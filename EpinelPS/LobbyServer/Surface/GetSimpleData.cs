using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Hexacode;

[PacketPath("/surface/lobby/simpledata")]
public class GetSimpleData : LobbyMsgHandler
{
    protected override async Task HandleAsync()
    {
        ReqLobbySurfaceSimpleData req = await ReadData<ReqLobbySurfaceSimpleData>();
        User user = GetUser();

        ResLobbySurfaceSimpleData response = new();

        // TODO

        await WriteDataAsync(response);
    }
}
