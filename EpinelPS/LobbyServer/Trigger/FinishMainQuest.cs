using EpinelPS.Database;
using EpinelPS.StaticInfo;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Trigger
{
    [PacketPath("/Trigger/FinMainQuest")]
    public class FinishMainQuest : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqFinMainQuest>();
            var user = GetUser();
            Console.WriteLine("Complete quest: " + req.Tid);
            user.SetQuest(req.Tid, false);

            var completedQuest = GameData.Instance.GetMainQuestByTableId(req.Tid);
            if (completedQuest == null) throw new Exception("Quest not found");

            JsonDb.Save();
            var response = new ResFinMainQuest();
            await WriteDataAsync(response);
        }
    }
}
