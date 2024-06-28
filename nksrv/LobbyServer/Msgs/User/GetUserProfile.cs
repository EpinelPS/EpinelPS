using nksrv.Utils;
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

            var response = new ResGetProfileData();

            Console.WriteLine(req.TargetUsn);
            response.Data = new NetProfileData();
            WriteData(response);
        }
    }
}
