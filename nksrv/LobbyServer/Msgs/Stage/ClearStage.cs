using nksrv.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nksrv.LobbyServer.Msgs.Stage
{
    [PacketPath("/stage/clearstage")]
    public class ClearStage : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqClearStage>();

            var response = new ResClearStage();
            var user = GetUser();

            // TOOD: save to user info
            Console.WriteLine($"Stage " + req.StageId + " completed, result is " + req.BattleResult);

            if (req.BattleResult == 1)
            {
                user.LastStageCleared = req.StageId;

                if (user.FieldInfo.Count == 0)
                {
                    user.FieldInfo.Add(0, new FieldInfo() {  });
                }

                // TODO: figure out how stageid corresponds to chapter
                user.FieldInfo[0].CompletedStages.Add(new NetFieldStageData() { StageId = req.StageId });
                JsonDb.Save();
            }

            WriteData(response);
        }
    }
}
