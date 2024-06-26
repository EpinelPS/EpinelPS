using nksrv.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nksrv.LobbyServer.Msgs.Stage
{
    [PacketPath("/stage/get")]
    public class GetStage : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqGetStageData>();
            var user = GetUser();

            var response = new ResGetStageData();
            response.Field = new NetFieldObjectData();

            // locate chapter
            bool found = false;

            retry:
            foreach (var item in user.FieldInfo)
            {
                if (item.Key == req.Chapter)
                {
                    found = true;
                    foreach (var stage in item.Value.CompletedStages)
                    {
                        response.Field.Stages.Add(stage);
                    }
                    response.HasChapterBossEntered = item.Value.BossEntered;
                    break;
                }
            }

            if (!found)
            {
                Console.WriteLine("chapter not found: " + req.Chapter);

                user.FieldInfo.Add(req.Chapter, new FieldInfo());
                goto retry;
            }
           
      
            response.SquadData = "";


            WriteData(response);
        }
    }
}
