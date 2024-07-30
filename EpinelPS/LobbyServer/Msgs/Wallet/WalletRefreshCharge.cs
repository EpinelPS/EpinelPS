using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Msgs.Wallet
{
    [PacketPath("/wallet/refreshcharge")]
    public class WalletRefreshCharge : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqRefreshChargeCurrencyData>();
            var user = GetUser();

            var response = new ResRefreshChargeCurrencyData();
            response.FreeCash = new();
            response.ChargeCash = new();

            foreach (var item in user.Currency)
            {
                if (item.Key == CurrencyType.FreeCash)
                {
                    response.FreeCash.Type = (int)item.Key;
                    response.FreeCash.Value = item.Value;
                }
                else if (item.Key == CurrencyType.ChargeCash)
                {
                    response.ChargeCash.Type = (int)item.Key;
                    response.ChargeCash.Value = item.Value;
                }
            }
            await WriteDataAsync(response);
        }
    }
}
