using EpinelPS.Database;

namespace EpinelPS.LobbyServer.LobbyUser;

[GameRequest("/user/setnickname")]
public class SetNickname : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqSetNickname req = await ReadData<ReqSetNickname>();
        User user = GetUser();
        user.Nickname = req.Nickname;

        ResSetNickname response = new()
        {
            Result = SetNicknameResult.Okay,
            Nickname = req.Nickname
        };

        JsonDb.Save();

        await WriteDataAsync(response);
    }
}
