namespace EpinelPS.LobbyServer.LobbyUser;

[GameRequest("/user/speciallobbyslot/get")]
public class GetSpecialLobbySlotData : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqGetSpecialLobbySlotData req = await ReadData<ReqGetSpecialLobbySlotData>();

        ResGetSpecialLobbySlotData response = new();
        // TODO
        await WriteDataAsync(response);
    }
}
