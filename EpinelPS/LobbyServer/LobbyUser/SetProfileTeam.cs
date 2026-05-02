using EpinelPS.Database;

namespace EpinelPS.LobbyServer.LobbyUser;

[GameRequest("/user/setprofileteam")]
public class SetProfileTeam : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqSetProfileTeam req = await ReadData<ReqSetProfileTeam>();
        User user = GetUser();
        for (int i = 0; i < req.Team.Slots.Count - 1; i++)
        {
            NetTeamSlot slot = req.Team.Slots[i];

            user.RepresentationTeamDataNew[i] = slot.Value;
        }

        JsonDb.Save();
        ResSetProfileTeam response = new();

        await WriteDataAsync(response);
    }
}
