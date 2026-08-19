using EpinelPS.Database;
using Microsoft.EntityFrameworkCore;

namespace EpinelPS.LobbyServer.LobbyUser;

[GameRequest("/user/setprofiledesc")]
public class SetProfileDescription : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqSetProfileDesc req = await ReadData<ReqSetProfileDesc>();
        GameContext.Users.Where(u => u.ID == UserId).ExecuteUpdate(setters => setters.SetProperty(u => u.Description, req.Desc));

        var response = new ResSetProfileDesc()
        {
            Result = SetProfileDescResult.Okay,
        };
        await WriteDataAsync(response);
    }
}
