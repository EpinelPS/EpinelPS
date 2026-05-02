namespace EpinelPS.LobbyServer.Campaign;

[GameRequest("/campaign/getfieldobjectitemsnum")]
public class GetFieldObjectsCount : LobbyMessage
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
                Count = map.Value.CompletedObjects.Where(x => x.Type == 1).Count()
            });
        }

        await WriteDataAsync(response);
    }
}
