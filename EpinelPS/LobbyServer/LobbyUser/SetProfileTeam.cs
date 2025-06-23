using EpinelPS.Database;
using EpinelPS.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EpinelPS.LobbyServer.LobbyUser
{
    [PacketPath("/user/setprofileteam")]
    public class SetProfileTeam : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqSetProfileTeam>();
            var user = GetUser();
            for (int i = 0; i < req.Team.Slots.Count - 1; i++)
            {
                var slot = req.Team.Slots[i];

                user.RepresentationTeamDataNew[i] = slot.Value;
            }

            JsonDb.Save();
            var response = new ResSetProfileTeam();

            await WriteDataAsync(response);
        }
    }
}
