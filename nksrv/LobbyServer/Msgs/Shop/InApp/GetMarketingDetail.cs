using nksrv.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nksrv.LobbyServer.Msgs.Shop.InApp
{
    [PacketPath("/inappshop/jupiter/getmarketingdetail")]
    public class GetMarketingDetail : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqGetJupiterMarketingDetail>();
            var response = new ResGetJupiterMarketingDetail();
            response.MarketingDetail = "{}";

            WriteData(response);
        }
    }
}
