using EpinelPS.Database;
using EpinelPS.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EpinelPS.LobbyServer.LobbyUser
{
    [PacketPath("/user/setprofiledata")]
    public class SetProfileData : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqSetProfileData>();
            var user = GetUser();
            user.ProfileIconId = req.Icon;
            user.ProfileIconIsPrism = req.IsPrism;
            user.ProfileFrame = req.Frame;

            JsonDb.Save();
            var response = new ResSetProfileData();

            await WriteDataAsync(response);
        }
    }
}
