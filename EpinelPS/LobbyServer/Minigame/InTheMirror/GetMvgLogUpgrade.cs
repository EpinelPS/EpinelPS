using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Minigame.InTheMirror
{
    [PacketPath("/arcade/mvg/log/upgrade")]
    public class GetMvgLogUpgrade : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            await ReadData<ReqBuyFromArcadeMvgUpgradeShop>();

            await WriteDataAsync(new ResBuyFromArcadeMvgUpgradeShop() );
        }
    }
}
