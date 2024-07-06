using nksrv.Utils;
using Swan.Logging;
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
            
            response.Field = CreateFieldInfo(user, req.Chapter - 1, req.Mod == 0 ? "Normal" : "Hard");

            response.HasChapterBossEntered = true;

            response.SquadData = "";

            WriteData(response);
        }

        public static NetFieldObjectData CreateFieldInfo(Utils.User user, int chapter, string mod)
        {
            var f = new NetFieldObjectData();
            bool found = false;
            string key = chapter + "_" + mod;
            foreach (var item in user.FieldInfo)
            {
                if (item.Key == key)
                {
                    found = true;
                    foreach (var stage in item.Value.CompletedStages)
                    {
                        f.Stages.Add(stage);
                    }
                    foreach (var obj in item.Value.CompletedObjects)
                    {
                        f.Objects.Add(obj);
                    }
                    break;
                }
            }

            if (!found)
            {
                Console.WriteLine("chapter not found: " + key);

                user.FieldInfo.Add(key, new FieldInfo());
                return CreateFieldInfo(user, chapter, mod);
            }

            return f;
        }
    }
}
