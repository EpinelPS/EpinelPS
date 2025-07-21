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
            ReqSetProfileData req = await ReadData<ReqSetProfileData>();
            User user = GetUser();
            user.ProfileIconId = req.Icon;
            user.ProfileIconIsPrism = req.IsPrism;
            user.ProfileFrame = req.Frame;

            JsonDb.Save();
            ResSetProfileData response = new();

            await WriteDataAsync(response);
        }
    }
}
