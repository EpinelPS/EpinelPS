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
}
