using EpinelPS.Database;
using Microsoft.EntityFrameworkCore;

namespace EpinelPS.LobbyServer.LobbyUser;

[GameRequest("/user/setnickname")]
public class SetNickname : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqSetNickname req = await ReadData<ReqSetNickname>();
        GameContext.Users.Where(u => u.ID == UserId).ExecuteUpdate(setters => setters.SetProperty(u => u.Nickname, req.Nickname));

        ResSetNickname response = new()
        {
            Result = SetNicknameResult.Okay,
            Nickname = req.Nickname
        };

        await WriteDataAsync(response);
    }
}
