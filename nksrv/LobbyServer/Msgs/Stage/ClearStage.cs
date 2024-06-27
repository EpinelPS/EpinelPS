using nksrv.Utils;
using Swan.Logging;
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
                user.FieldInfo[GetChapterForStageId(req.StageId)].CompletedStages.Add(new NetFieldStageData() { StageId = req.StageId });
                JsonDb.Save();
            }

            WriteData(response);
        }

        public static int GetChapterForStageId(int stageId)
        {
            if (6000001 <= stageId && stageId <= 6000003)
            {
                return 0;
            }
            else if (6001001 <= stageId && stageId <= 6001004)
            {
                return 1;
            }
            else
            {
                Logger.Error("Unknown stage id: " + stageId);
                return 100;
            }
        }
    }
}
