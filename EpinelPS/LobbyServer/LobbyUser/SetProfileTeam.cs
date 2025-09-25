using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.LobbyUser
{
    [PacketPath("/user/setprofileteam")]
    public class SetProfileTeam : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqSetProfileTeam req = await ReadData<ReqSetProfileTeam>();
            User user = GetUser();
            List<long> values = [];
            foreach (NetTeamSlot slot in req.Team.Slots)
            {
                values.Add(slot.Value);
            }
            user.RepresentationTeamDataNew = [.. values];
            JsonDb.Save();
            ResSetProfileTeam response = new();

            await WriteDataAsync(response);
        }
    }
}
