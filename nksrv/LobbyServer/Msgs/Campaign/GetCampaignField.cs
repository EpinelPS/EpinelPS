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
            response.Field = new NetFieldObjectData();

            // todo save this data
            response.Team = new NetUserTeamData() { LastContentsTeamNumber = 1, Type = 1 };

            string resultingJson;
            if (!user.MapJson.ContainsKey(req.MapId))
            {
                resultingJson = "{\"_mapID\":\"" + req.MapId + "\",\"_triggerDataList\":[],\"_interactionActionTriggerDataList\":[],\"_interactionObjectDataList\":[],\"_questObjectDataList\":[],\"_togglePortalDataList\":[],\"_squadDataList\":[{\"UniqueKey\":\"a365d3ab-2961-4eb8-940b-f68629957b48\",\"IsValid\":true,\"TeamType\":1,\"Number\":1,\"Position\":{\"x\":-14.899999618530274,\"y\":0.08333173394203186,\"z\":-3.2200000286102297}}]}";
                user.MapJson.Add(req.MapId, resultingJson);
            }
            else
            {
                resultingJson = user.MapJson[req.MapId];
            }


            WriteData(response);
        }
    }
}
