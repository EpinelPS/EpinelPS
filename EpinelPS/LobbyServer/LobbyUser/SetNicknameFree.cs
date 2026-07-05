using EpinelPS.Database;
using Microsoft.EntityFrameworkCore;

namespace EpinelPS.LobbyServer.LobbyUser;

[GameRequest("/user/setnicknamefree")]
public class SetNicknameFree : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqSetNicknameFree req = await ReadData<ReqSetNicknameFree>();
        GameContext.Users.Where(u => u.ID == UserId).ExecuteUpdate(setters => setters.SetProperty(u => u.Nickname, req.Nickname));

        ResSetNicknameFree response = new()
        {
            Result = SetNicknameResult.Okay,
            Nickname = req.Nickname
        };

        await WriteDataAsync(response);
    }
}
