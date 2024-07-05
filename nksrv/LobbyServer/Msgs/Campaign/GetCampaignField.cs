using nksrv.LobbyServer.Msgs.Stage;
using nksrv.StaticInfo;
using nksrv.Utils;
using Swan.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nksrv.LobbyServer.Msgs.Campaign
{
    [PacketPath("/campaign/getfield")]
    public class GetCampaignField : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqGetCampaignFieldData>();
            var user = GetUser();
           
            Console.WriteLine("Map ID: " + req.MapId);

            var response = new ResGetCampaignFieldData();
            response.Field = GetStage.CreateFieldInfo(user, StaticDataParser.Instance.GetNormalChapterNumberFromFieldName(req.MapId), req.MapId.Contains("hard") ? "Hard" : "Normal");

            // todo save this data
            response.Team = new NetUserTeamData() { LastContentsTeamNumber = 1, Type = 1 };
            if (user.LastNormalStageCleared >= 6000003)
            {
                var team = new NetTeamData() { TeamNumber = 1 };
                team.Slots.Add(new NetTeamSlot() { Slot = 1, Value = 47263455 });
                team.Slots.Add(new NetTeamSlot() { Slot = 2, Value = 47263456 });
                team.Slots.Add(new NetTeamSlot() { Slot = 3, Value = 47263457 });
                team.Slots.Add(new NetTeamSlot() { Slot = 4, Value = 47263458 });
                team.Slots.Add(new NetTeamSlot() { Slot = 5, Value = 47263459 });
                response.Team.Teams.Add(team);

                response.TeamPositions.Add(new NetCampaignTeamPosition() { TeamNumber = 1, Type = 1, Position = new NetVector3() { } });
            }

            string resultingJson;
            if (!user.MapJson.ContainsKey(req.MapId))
            {
                resultingJson = "";
                user.MapJson.Add(req.MapId, resultingJson);
            }
            else
            {
                resultingJson = user.MapJson[req.MapId];
            }

            response.Json = resultingJson;

            WriteData(response);
        }
    }
}
