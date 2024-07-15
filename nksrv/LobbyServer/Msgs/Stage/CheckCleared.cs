using nksrv.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nksrv.LobbyServer.Msgs.Stage
{
    [PacketPath("/stage/checkclear")]
    public class CheckCleared : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqCheckStageClear>();

            var response = new ResCheckStageClear();
            var user = GetUser();
            Console.WriteLine("CheckClear len: " + req.StageIds.Count);

            // TODO: is this correct

            foreach (var fields in user.FieldInfoNew)
            {
                foreach (var stages in fields.Value.CompletedStages)
                {
                    response.ClearedStageIds.Add(stages);
                }
            }



          await  WriteDataAsync(response);
        }
    }

}
