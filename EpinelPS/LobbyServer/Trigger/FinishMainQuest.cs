using EpinelPS.Data;
using EpinelPS.Database;

namespace EpinelPS.LobbyServer.TriggerController;

[GameRequest("/Trigger/FinMainQuest")]
public class FinishMainQuest : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqFinMainQuest req = await ReadData<ReqFinMainQuest>();
        User user = GetUser();
        Console.WriteLine("Complete quest: " + req.Tid);
        user.SetQuest(req.Tid, false);

        var completedQuest = GameData.Instance.GetMainQuestByTableId(req.Tid) ?? throw new Exception("Quest not found");

        user.AddTrigger(Trigger.CampaignClear, 1, completedQuest.ConditionId[0].ConditionId);
        user.AddTrigger(Trigger.MainQuestClear, 1, completedQuest.Id);

        JsonDb.Save();
        ResFinMainQuest response = new();
        await WriteDataAsync(response);
    }
}
