using nksrv.LobbyServer.Msgs.Stage;
using nksrv.Utils;
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
            response.Field = GetStage.CreateFieldInfo(user, 0); // TODO dont hardcode chapter

            // todo save this data
            response.Team = new NetUserTeamData() { LastContentsTeamNumber = 1, Type = 1 };

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
