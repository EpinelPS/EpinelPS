using nksrv.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nksrv.LobbyServer.Msgs.Character
{
    [PacketPath("/character/costume/get")]
    public class GetCharacterCostume : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = ReadData<ReqGetCharacterCostumeData>();

            var response = new ResGetCharacterCostumeData();

            // TODO implement

            WriteData(response);
        }
    }
}
