using EpinelPS.Database;
using EpinelPS.LobbyServer.Stage;
using EpinelPS.Data;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Campaign
{
    [PacketPath("/campaign/getfieldobjectitemsnum")]
    public class GetFieldObjectsCount : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqGetCampaignFieldObjectItemsNum req = await ReadData<ReqGetCampaignFieldObjectItemsNum>();
            User user = GetUser();

            ResGetCampaignFieldObjectItemsNum response = new();

            foreach (KeyValuePair<string, FieldInfoNew> map in user.FieldInfoNew)
            {
                response.FieldObjectItemsNum.Add(new NetCampaignFieldObjectItemsNum()
                {
                    MapId = map.Key,
                    Count = map.Value.CompletedObjects.Count
                });
            }

            await WriteDataAsync(response);
        }
    }
}
