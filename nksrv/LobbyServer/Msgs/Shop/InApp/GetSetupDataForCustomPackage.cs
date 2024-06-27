using nksrv.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nksrv.LobbyServer.Msgs.Shop.InApp
{
    [PacketPath("/inappshop/custompackage/getsetupdata")]
    public class GetCharacterAttractiveList : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqGetCustomPackageSetupData>();

            var response = new ResGetCustomPackageSetupData();

            // TODO: Validate response from real server and pull info from user info
            WriteData(response);
        }
    }
}
