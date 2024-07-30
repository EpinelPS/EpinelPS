using EpinelPS.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EpinelPS.LobbyServer.Msgs.User
{
    [PacketPath("/user/speciallobbyslot/get")]
    public class GetSpecialLobbySlotData : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqGetSpecialLobbySlotData>();

            var response = new ResGetSpecialLobbySlotData();
            // TODO
            await WriteDataAsync(response);
        }
    }
}
