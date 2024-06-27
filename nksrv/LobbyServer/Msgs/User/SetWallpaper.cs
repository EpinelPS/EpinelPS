using nksrv.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nksrv.LobbyServer.Msgs.User
{
    [PacketPath("/User/SetWallpaper")]
    public class SetWallpaper : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqSetWallpaper>();
            var response = new ResSetWallpaper();
            // todo

            WriteData(response);
        }
    }
}
