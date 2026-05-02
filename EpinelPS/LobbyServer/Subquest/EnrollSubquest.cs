using EpinelPS.Data;
using EpinelPS.Database;

namespace EpinelPS.LobbyServer.Subquest;

[GameRequest("/subquest/enrollment")]
public class EnrollSubquest : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqEnrollmentSubQuest req = await ReadData<ReqEnrollmentSubQuest>();
        User user = GetUser();

        ResEnrollmentSubQuest response = new();

        if (!GameData.Instance.Subquests.TryGetValue(req.SubquestId, out _))
            throw new Exception("no such subquest: " + req.SubquestId);

        if (!user.SubQuestData.ContainsKey(req.SubquestId))
            user.SetSubQuest(req.SubquestId, false);

        response.SubquestData = new NetSubQuestData()
        {
            CreatedAt = DateTime.UtcNow.Ticks,
            IsReceived = false,
            SubQuestId = req.SubquestId
        };

        JsonDb.Save();

        await WriteDataAsync(response);
    }
}
