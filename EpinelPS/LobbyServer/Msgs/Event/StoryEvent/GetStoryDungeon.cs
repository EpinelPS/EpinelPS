using EpinelPS.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EpinelPS.LobbyServer.Msgs.Event.StoryEvent
{
    [PacketPath("/event/storydungeon/get")]
    public class GetStoryDungeon : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqStoryDungeonEventData>();
            var user = GetUser();

            var response = new ResStoryDungeonEventData()
            {
                TeamData = new NetUserTeamData(),
                RemainingTickets = 10,
            };

            // TOOD

            await WriteDataAsync(response);
        }
    }
}
