using nksrv.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nksrv.LobbyServer.Msgs.Shop.InApp
{
    [PacketPath("/inappshop/getdata")]
    public class GetProductList : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var x = await ReadData<ReqGetInAppShopData>();

            var response = new ResGetInAppShopData();

            // TODO

          await  WriteDataAsync(response);
        }
    }
}
