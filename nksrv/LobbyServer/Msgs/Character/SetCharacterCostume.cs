using nksrv.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nksrv.LobbyServer.Msgs.Character
{
    [PacketPath("/character/costume/set")]
    public class SetCharacterCostume : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqSetCharacterCostume>();
            var user = GetUser();

            foreach (var item in user.Characters)
            {
                if (item.Csn == req.Csn)
                {
                    item.CostumeId = req.CostumeId;
                    break;
                }
            }
            JsonDb.Save();

            var response = new ResSetCharacterCostume();

            await WriteDataAsync(response);
        }
    }
}
