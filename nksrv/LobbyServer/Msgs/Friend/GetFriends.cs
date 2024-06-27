using nksrv.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nksrv.LobbyServer.Msgs.Friend
{
    [PacketPath("/friend/get")]
    public class GetFriends : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqGetFriendData>();
            var response = new ResGetFriendData();


            WriteData(response);
        }
    }
}
