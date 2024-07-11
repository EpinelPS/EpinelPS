using nksrv.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nksrv.LobbyServer.Msgs.User
{
    [PacketPath("/user/setprofileicon")]
    public class SetProfileIcon : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqSetProfileIcon>();
            var user = GetUser();
            user.ProfileIconId = req.Icon;
            user.ProfileIconIsPrism = req.IsPrism;
            JsonDb.Save();
            var response = new ResSetProfileIcon();

            await WriteDataAsync(response);
        }
    }
}
