namespace EpinelPS.LobbyServer.Campaign;

[GameRequest("/campaign/getfieldobjectitemsnum")]
public class GetFieldObjectsCount : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqGetCampaignFieldObjectItemsNum req = await ReadData<ReqGetCampaignFieldObjectItemsNum>();
        User user = GetUser();

        ResGetCampaignFieldObjectItemsNum response = new();

        foreach (var map in user.FieldInfo)
        {
            response.FieldObjectItemsNum.Add(new NetCampaignFieldObjectItemsNum()
            {
                MapId = map.MapName,
                Count = map.CompletedObjects.Where(x => x.Type == 1).Count()
            });
        }

        await WriteDataAsync(response);
    }
}
