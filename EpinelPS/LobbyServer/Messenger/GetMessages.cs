using EpinelPS.Data;

namespace EpinelPS.LobbyServer.Messenger;

[GameRequest("/messenger/get")]
public class GetMessages : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqGetMessages req = await ReadData<ReqGetMessages>();
        User user = GetUser();

        ResGetMessages response = new();

        foreach (NetMessage message in user.MessengerData.Where(message => message.Seq >= req.Seq))
        {
            response.Messages.Add(message);
        }

        await WriteDataAsync(response);
    }

}
