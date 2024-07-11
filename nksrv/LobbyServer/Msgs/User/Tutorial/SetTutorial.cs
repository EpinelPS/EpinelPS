using nksrv.StaticInfo;
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

            if (!user.ClearedTutorialData.ContainsKey(req.LastClearedTid))
            {
                var cleared = StaticDataParser.Instance.GetTutorialDataById(req.LastClearedTid);
                cleared.id = req.LastClearedTid;
                user.ClearedTutorialData.Add(req.LastClearedTid, cleared);
            }
            JsonDb.Save();

            var response = new ResSetTutorial();
            await WriteDataAsync(response);
        }
    }
}
