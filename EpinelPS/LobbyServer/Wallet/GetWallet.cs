using EpinelPS.Data;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Wallet
{
    [PacketPath("/wallet/get")]
    public class GetWallet : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqGetCurrencyData req = await ReadData<ReqGetCurrencyData>();
            Database.User user = GetUser();

            ResGetCurrencyData response = new();

            foreach (KeyValuePair<CurrencyType, long> currency in user.Currency)
            {
                response.Currency.Add(new NetUserCurrencyData() { Type = (int)currency.Key, Value = currency.Value });
            }

            await WriteDataAsync(response);
        }
    }
}
