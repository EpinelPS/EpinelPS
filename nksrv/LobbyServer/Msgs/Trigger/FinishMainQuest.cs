using nksrv.StaticInfo;
using nksrv.Utils;

namespace nksrv.LobbyServer.Msgs.Trigger
{
    [PacketPath("/Trigger/FinMainQuest")]
    public class FinishMainQuest : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqFinMainQuest>();
            var user = GetUser();
            Console.WriteLine("Complete quest: " + req.Tid);
            user.SetQuest(req.Tid, true);

            var completedQuest = StaticDataParser.Instance.GetMainQuestByTableId(req.Tid);
            if (completedQuest == null) throw new Exception("Quest not found");

            JsonDb.Save();
            var response = new ResFinMainQuest();
            await WriteDataAsync(response);
        }
    }
}
