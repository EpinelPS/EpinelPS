using nksrv.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nksrv.LobbyServer.Msgs.Wallet
{
    [PacketPath("/wallet/get")]
    public class GetWallet : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqGetCurrencyData>();
            var response = new ResGetCurrencyData();
            foreach (var item in req.Currencies)
            {
                Console.WriteLine("Request currency " + (CurrencyType)item);
            }

            WriteData(response);
        }
    }
}
