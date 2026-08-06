using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Gacha;

[GameRequest("/gacha/setselectup")]
public class SetGachaSelectSetup : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqSetGachaSelectup req = await ReadData<ReqSetGachaSelectup>();
        User user = GetUser();

        ResSetGachaSelectup response = new ResSetGachaSelectup();

        var data = req.GachaSelectupData;

        if (user.GachaSelectupChoices.ContainsKey(req.GachaSelectupData.GachaTypeId))
            user.GachaSelectupChoices[req.GachaSelectupData.GachaTypeId] = req.GachaSelectupData.GachaSelectupId;
        else
            user.GachaSelectupChoices.Add(req.GachaSelectupData.GachaTypeId, req.GachaSelectupData.GachaSelectupId);

        JsonDb.Save();

        Logging.WriteLine($"[SetGachaSelectSetup] Updated selection for user {user.ID} to GachaTypeId {req.GachaSelectupData.GachaTypeId} - GachaSelectupId {user.GachaSelectupChoices[req.GachaSelectupData.GachaTypeId]}");

        // Write the response back
        await WriteDataAsync(response);
    }
}
