namespace EpinelPS.LobbyServer.Gacha;

[GameRequest("/gacha/setselectup")]
public class SetSelectup : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqSetGachaSelectup req = await ReadData<ReqSetGachaSelectup>();
        User user = GetUser();

        if (req.GachaSelectupData != null &&
            EpinelPS.Data.GameData.Instance.GachaSelectupList.TryGetValue(req.GachaSelectupData.GachaSelectupId, out EpinelPS.Data.GachaSelectupListRecord_Raw? selectup) &&
            selectup.GachaTypeId == req.GachaSelectupData.GachaTypeId)
        {
            user.GachaSelectupSelections[req.GachaSelectupData.GachaTypeId] = req.GachaSelectupData.GachaSelectupId;
            EpinelPS.Database.JsonDb.Save();
        }

        await WriteDataAsync(new ResSetGachaSelectup());
    }
}
