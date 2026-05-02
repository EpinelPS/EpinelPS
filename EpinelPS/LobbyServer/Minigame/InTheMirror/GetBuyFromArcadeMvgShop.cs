namespace EpinelPS.LobbyServer.Minigame.InTheMirror;

[GameRequest("/arcade/mvg/log/shop")]
public class GetBuyFromArcadeMvgShop : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        await ReadData<ReqBuyFromArcadeMvgShop>();

        await WriteDataAsync(new ResBuyFromArcadeMvgShop());
    }
}
