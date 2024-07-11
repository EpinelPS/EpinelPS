using nksrv.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nksrv.LobbyServer.Msgs.User
{
    [PacketPath("/User/GetProfileFrame")]
    public class GetProfileFrame : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqGetProfileFrame>();
            var response = new ResGetProfileFrame();

           
          await  WriteDataAsync(response);
        }
    }
}
