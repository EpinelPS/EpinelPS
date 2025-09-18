using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Minigame.InTheMirror
{
    [PacketPath("/arcade/mvg/log/shop")]
    public class GetBuyFromArcadeMvgShop : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            await ReadData<ReqBuyFromArcadeMvgShop>();

            await WriteDataAsync(new ResBuyFromArcadeMvgShop());
        }
    }
}
