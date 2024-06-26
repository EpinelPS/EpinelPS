using nksrv.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nksrv.LobbyServer.Msgs.User
{
    [PacketPath("/user/setnickname")]
    public class SetNickname : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqSetNickname>();
            var user = GetUser();
            user.Nickname = req.Nickname;

            var response = new ResSetNickname();
            response.Result = SetNicknameResult.NicknameOk;
            response.Nickname = req.Nickname;
                
            WriteData(response);
        }
    }
}
