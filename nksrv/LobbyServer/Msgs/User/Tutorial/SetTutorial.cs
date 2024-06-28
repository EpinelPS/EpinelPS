using nksrv.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nksrv.LobbyServer.Msgs.User.Tutorial
{
    [PacketPath("/tutorial/set")]
    public class SetTutorial : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqSetTutorial>();
            var user = GetUser();
            if (!user.ClearedTutorials.Contains(req.LastClearedTid))
            user.ClearedTutorials.Add(req.LastClearedTid);

            var response = new ResSetTutorial();
            WriteData(response);
        }
    }
}
