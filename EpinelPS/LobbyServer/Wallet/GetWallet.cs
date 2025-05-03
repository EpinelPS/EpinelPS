using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Wallet
{
    [PacketPath("/wallet/get")]
    public class GetWallet : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqGetCurrencyData>();
            var user = GetUser();

            var response = new ResGetCurrencyData();

            foreach (var currency in user.Currency)
            {
                response.Currency.Add(new NetUserCurrencyData() { Type = (int)currency.Key, Value = currency.Value });
            }

            await WriteDataAsync(response);
        }
    }
}
