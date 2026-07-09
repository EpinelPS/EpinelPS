namespace EpinelPS.LobbyServer.Campaign;

[GameRequest("/campaign/savefield")]
public class SaveField : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqSaveCampaignField req = await ReadData<ReqSaveCampaignField>();
        User user = GetUser();

        ResSaveCampaignField response = new();

        var field = user.FieldInfo.FirstOrDefault(f => f.MapName == req.MapId);

        if (field == null)
        {
            field = new FieldInfoNew
            {
                MapName = req.MapId
            };
            user.FieldInfo.Add(field);
        }

        field.PositionJson = req.Json;

        await GameContext.SaveChangesAsync();

        await WriteDataAsync(response);
    }
}
