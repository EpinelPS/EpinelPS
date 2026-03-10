using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Surface;

[PacketPath("/Surface/HasFinishedTutorial")]
public class HasFinishedTutorial : LobbyMsgHandler
{
    protected override async Task HandleAsync()
    {
        ReqHasFinishedSurfaceTutorial req = await ReadData<ReqHasFinishedSurfaceTutorial>();
        User user = User;

        ResHasFinishedSurfaceTutorial response = new();

        // TODO

        await WriteDataAsync(response);
    }
}
