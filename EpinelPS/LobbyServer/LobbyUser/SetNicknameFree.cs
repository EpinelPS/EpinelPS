using EpinelPS.Database;

namespace EpinelPS.LobbyServer.LobbyUser;

[GameRequest("/user/setnicknamefree")]
public class SetNicknameFree : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqSetNicknameFree req = await ReadData<ReqSetNicknameFree>();
        User user = GetUser();
        user.Nickname = req.Nickname;

        ResSetNicknameFree response = new()
        {
            Result = SetNicknameResult.Okay,
            Nickname = req.Nickname
        };

        JsonDb.Save();

        await WriteDataAsync(response);
    }
}
