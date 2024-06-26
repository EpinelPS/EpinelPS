using nksrv.Utils;

namespace nksrv.LobbyServer.Msgs.Wallet
{
    [PacketPath("/wallet/refreshcharge")]
    public class WalletRefreshCharge : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqRefreshChargeCurrencyData>();


            var response = new ResRefreshChargeCurrencyData();
            WriteData(response);
        }
    }
}
