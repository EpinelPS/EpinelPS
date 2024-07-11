using nksrv.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nksrv.LobbyServer.Msgs.Messenger
{
    [PacketPath("/messenger/picked/get")]
    public class GetPickedMessage : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqGetPickedMessageList>();

            // TODO: get proper response
            var response = new ResGetPickedMessageList();

          await  WriteDataAsync(response);
        }
    }
}
