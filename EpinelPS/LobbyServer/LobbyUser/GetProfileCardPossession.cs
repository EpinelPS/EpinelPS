namespace EpinelPS.LobbyServer.LobbyUser;

[GameRequest("/ProfileCard/Possession/Get")]
public class GetProfileCardPossession : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqProfileCardObjectList req = await ReadData<ReqProfileCardObjectList>();

        ResProfileCardObjectList response = new();
        // TODO
        await WriteDataAsync(response);
    }
}
