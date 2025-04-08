using EpinelPS.Database;
using EpinelPS.LobbyServer.Stage;
using EpinelPS.StaticInfo;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Campaign
{
    [PacketPath("/campaign/getfieldobjectitemsnum")]
    public class GetFieldObjectsCount : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqGetCampaignFieldObjectItemsNum>();
            var user = GetUser();

            var response = new ResGetCampaignFieldObjectItemsNum();

            foreach (var map in user.FieldInfoNew)
            {
                response.FieldObjectItemsNum.Add(new NetCampaignFieldObjectItemsNum()
                {
                    MapId = GameData.Instance.GetMapIdFromDBFieldName(map.Key),
                    Count = map.Value.CompletedObjects.Count
                });
            }

            await WriteDataAsync(response);
        }
    }
}
