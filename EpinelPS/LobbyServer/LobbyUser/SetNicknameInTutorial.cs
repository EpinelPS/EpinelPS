using EpinelPS.Database;
using Microsoft.EntityFrameworkCore;

namespace EpinelPS.LobbyServer.LobbyUser;

[GameRequest("/User/SetNickNameInTutorial")]
public class SetNicknameInTutorial : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqSetNicknameInTutorial req = await ReadData<ReqSetNicknameInTutorial>();
        User user = GetUser();
        GameContext.Users.Where(u => u.ID == UserId).ExecuteUpdate(setters => setters.SetProperty(u => u.Nickname, req.Nickname));

        ResSetNicknameInTutorial response = new()
        {
            Result = SetNicknameResult.Okay,
            Nickname = req.Nickname
        };

        await WriteDataAsync(response);
    }
}
