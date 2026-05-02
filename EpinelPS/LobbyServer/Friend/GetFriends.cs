namespace EpinelPS.LobbyServer.Friend;

[GameRequest("/friend/get")]
public class GetFriends : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqGetFriendData req = await ReadData<ReqGetFriendData>();
        ResGetFriendData response = new();


        await WriteDataAsync(response);
    }
}
