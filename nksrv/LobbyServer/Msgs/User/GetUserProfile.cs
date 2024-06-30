using nksrv.Utils;
using Swan.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nksrv.LobbyServer.Msgs.User
{
    [PacketPath("/User/GetProfile")]
    public class GetUserProfile : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqGetProfileData>();
            var user = GetUser();
            var response = new ResGetProfileData();
            response.Data = new NetProfileData();
            Console.WriteLine("GET USER PROFILE NOT IMPLEMENTED: " + req.TargetUsn);
            if (user.ID == (ulong)req.TargetUsn)
            {
                response.Data.User = LobbyHandler.CreateWholeUserDataFromDbUser(user);
            }
            else
            {
                Logger.Warn("Unknown User ID: " + req.TargetUsn);
            }
         
            WriteData(response);
        }
    }
}
