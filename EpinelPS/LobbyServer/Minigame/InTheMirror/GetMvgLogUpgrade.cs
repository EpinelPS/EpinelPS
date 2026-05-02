namespace EpinelPS.LobbyServer.Minigame.InTheMirror;

[GameRequest("/arcade/mvg/log/upgrade")]
public class GetMvgLogUpgrade : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        await ReadData<ReqBuyFromArcadeMvgUpgradeShop>();

        await WriteDataAsync(new ResBuyFromArcadeMvgUpgradeShop());
    }
}
