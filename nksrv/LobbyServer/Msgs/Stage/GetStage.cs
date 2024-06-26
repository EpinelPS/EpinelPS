using nksrv.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Google.Rpc.Context.AttributeContext.Types;
using static System.Net.Mime.MediaTypeNames;

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
            response.Field = CreateFieldInfo(user, req.Chapter);
           
           
      
            response.SquadData = "";

            response.Field.Stages.Clear();
            WriteData(response);
        }

        public static NetFieldObjectData CreateFieldInfo(Utils.User user, int chapter)
        {
            var f = new NetFieldObjectData();
            bool found = false;
            foreach (var item in user.FieldInfo)
            {
                if (item.Key == chapter)
                {
                    found = true;
                    foreach (var stage in item.Value.CompletedStages)
                    {
                        f.Stages.Add(stage);
                    }
                    break;
                }
            }

            if (!found)
            {
                Console.WriteLine("chapter not found: " + chapter);

                user.FieldInfo.Add(chapter, new FieldInfo());
                return CreateFieldInfo(user, chapter);
            }

            return f;
        }
    }
}
