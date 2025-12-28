using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Surface;

[PacketPath("/Surface/Tutorial/Mission/Enter")]
public class EnterTutorialMission : LobbyMsgHandler
{
    protected override async Task HandleAsync()
    {
        ReqEnterSurfaceMissionTutorial req = await ReadData<ReqEnterSurfaceMissionTutorial>();
        User user = GetUser();

        ResEnterSurfaceMissionTutorial response = new()
        {
            
        };

        // TODO

        await WriteDataAsync(response);
    }
}
