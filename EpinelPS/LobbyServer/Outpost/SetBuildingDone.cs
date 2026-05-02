namespace EpinelPS.LobbyServer.Outpost;

[GameRequest("/outpost/buildingisdone")]
public class SetBuildingDone : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqBuildingIsDone req = await ReadData<ReqBuildingIsDone>();
        User user = GetUser();

        ResBuildingIsDone response = new();



        await WriteDataAsync(response);
    }
}
