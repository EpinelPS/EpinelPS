using nksrv.Utils;

namespace nksrv.LobbyServer.Msgs.Campaign
{
    [PacketPath("/campaign/savefield")]
    public class SaveField : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqSaveCampaignField>();
            var user = GetUser();

            var response = new ResGetFieldTalkList();

            Console.WriteLine($"save {req.MapId} with {req.Json}");

            if (!user.MapJson.ContainsKey(req.MapId))
            {
                user.MapJson.Add(req.MapId, req.Json);
            }
            else
            {
               user.MapJson[req.MapId] = req.Json;
            }

            await WriteDataAsync(response);
        }
    }
}
