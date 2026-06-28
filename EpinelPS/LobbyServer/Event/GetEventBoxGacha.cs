namespace EpinelPS.LobbyServer.Event;

[GameRequest("/event/boxgacha/get")]
public class GetEventBoxGacha : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        // from client: {"EventId":10051}
        ReqGetEventBoxGacha req = await ReadData<ReqGetEventBoxGacha>();
        User user = GetUser();

        ResGetEventBoxGacha response = EventBoxGachaHelper.Get(user, req.EventId);

        await WriteDataAsync(response);
    }
}
