namespace EpinelPS.LobbyServer.Messenger;

[GameRequest("/messenger/picked/get")]
public class GetPickedMessage : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqGetPickedMessageList req = await ReadData<ReqGetPickedMessageList>();

        // TODO: get proper response
        ResGetPickedMessageList response = new();

        await WriteDataAsync(response);
    }
}
