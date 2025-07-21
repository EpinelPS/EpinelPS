using EpinelPS.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EpinelPS.LobbyServer.LobbyUser
{
    [PacketPath("/user/speciallobbyslot/get")]
    public class GetSpecialLobbySlotData : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqGetSpecialLobbySlotData req = await ReadData<ReqGetSpecialLobbySlotData>();

            ResGetSpecialLobbySlotData response = new();
            // TODO
            await WriteDataAsync(response);
        }
    }
}
