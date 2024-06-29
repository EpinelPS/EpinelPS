using nksrv.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nksrv.LobbyServer.Msgs.User
{
    [PacketPath("/User/GetWallpaper")]
    public class GetWallpaper : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqGetWallpaper>();
            var response = new ResGetWallpaper();
            response.WallpaperList.AddRange(GetUser().WallpaperList);

            WriteData(response);
        }
    }
}
