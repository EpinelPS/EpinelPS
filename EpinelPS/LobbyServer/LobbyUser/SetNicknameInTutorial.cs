using EpinelPS.Database;

namespace EpinelPS.LobbyServer.LobbyUser;

[GameRequest("/User/SetNickNameInTutorial")]
public class SetNicknameInTutorial : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqSetNicknameInTutorial req = await ReadData<ReqSetNicknameInTutorial>();
        User user = GetUser();
        user.Nickname = req.Nickname;

        ResSetNicknameInTutorial response = new()
        {
            Result = SetNicknameResult.Okay,
            Nickname = req.Nickname
        };

        JsonDb.Save();

        await WriteDataAsync(response);
    }
}
