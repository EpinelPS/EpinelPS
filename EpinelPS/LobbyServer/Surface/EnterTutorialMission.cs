namespace EpinelPS.LobbyServer.Surface;

[GameRequest("/Surface/Tutorial/Mission/Enter")]
public class EnterTutorialMission : LobbyMessage
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
