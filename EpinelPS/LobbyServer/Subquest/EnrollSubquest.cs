using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.LobbyServer.Messenger;

namespace EpinelPS.LobbyServer.Subquest;

[GameRequest("/subquest/enrollment")]
public class EnrollSubquest : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqEnrollmentSubQuest req = await ReadData<ReqEnrollmentSubQuest>();
        User user = GetUser();

        ResEnrollmentSubQuest response = new();

        if (!GameData.Instance.Subquests.TryGetValue(req.SubquestId, out SubQuestRecord? subQuest))
            throw new Exception("no such subquest: " + req.SubquestId);

        if (subQuest.BeforeSubQuestId > 0 &&
            (!user.SubQuestData.TryGetValue(subQuest.BeforeSubQuestId, out bool previousCompleted) || !previousCompleted))
            throw new Exception("subquest prerequisite is not completed: " + subQuest.BeforeSubQuestId);

        if (!MessengerTriggerUtils.IsTriggerListSatisfied(user, subQuest.TriggerList))
            throw new Exception("subquest conditions are not satisfied: " + req.SubquestId);

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
