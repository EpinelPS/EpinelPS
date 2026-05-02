namespace EpinelPS.LobbyServer.Simroom;

[GameRequest("/simroom/enterbattle")]
public class EnterBattle : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        await ReadData<ReqEnterSimRoomBattle>();

        ResEnterSimRoomBattle response = new()
        {
            Result = SimRoomResult.Success
        };

        await WriteDataAsync(response);
    }
}