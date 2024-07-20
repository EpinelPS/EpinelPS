using nksrv.Net;
using nksrv.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nksrv.LobbyServer.Msgs.Shop
{
    [PacketPath("/shop/get")]
    public class GetShop : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var x = await ReadData<GetShopRequest>();

            var response = new GetShopResponse();

            // TODO

            await WriteDataAsync(response);
        }
    }
}
