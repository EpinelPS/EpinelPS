using nksrv.StaticInfo;
using nksrv.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

            // set next quest as available
            user.SetQuest(completedQuest.next_main_quest_id, true);

            JsonDb.Save();
            var response = new ResFinMainQuest();
            WriteData(response);
        }
    }
}
