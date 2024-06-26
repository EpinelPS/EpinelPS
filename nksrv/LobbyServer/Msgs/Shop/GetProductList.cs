using nksrv.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nksrv.LobbyServer.Msgs.Shop
{
    [PacketPath("/inappshop/jupiter/getproductlist")]
    public class GetProductList : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            try
            {
                var x = await ReadData<ReqGetJupiterProductList>(); //todo: causes crash

                var response = new ResGetJupiterProductList();
                foreach (var item in x.ProductIdList)
                {
                    response.ProductInfoList.Add(new NetJupiterProductInfo() { CurrencyCode = "US", CurrencySymbol = "$", MicroPrice = 212, Price = "22", ProductId = item });
                }
                WriteData(response);
            }
            catch
            {
                ;
            }
        }
    }
}
