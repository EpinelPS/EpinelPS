using nksrv.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nksrv.LobbyServer.Msgs.Shop.InApp
{
    [PacketPath("/inappshop/getreceivableproductlist")]
    public class GetRetrivableProductList : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var x = await ReadData<ReqGetInAppShopReceivableProductList>();

            var response = new ResGetInAppShopReceivableProductList();
            // TODO

          await  WriteDataAsync(response);
        }
    }
}
