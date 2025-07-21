using EpinelPS.Database;
using EpinelPS.Data;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Trigger
{
    [PacketPath("/Trigger/FinMainQuest")]
    public class FinishMainQuest : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqFinMainQuest req = await ReadData<ReqFinMainQuest>();
            User user = GetUser();
            Console.WriteLine("Complete quest: " + req.Tid);
            user.SetQuest(req.Tid, false);

            MainQuestCompletionRecord completedQuest = GameData.Instance.GetMainQuestByTableId(req.Tid) ?? throw new Exception("Quest not found");

            user.AddTrigger(TriggerType.CampaignClear, 1, completedQuest.condition_id);
            user.AddTrigger(TriggerType.MainQuestClear, 1, completedQuest.id);

            JsonDb.Save();
            ResFinMainQuest response = new();
            await WriteDataAsync(response);
        }
    }
}
