using EpinelPS.Data;

namespace EpinelPS.LobbyServer.Wallet;

[GameRequest("/wallet/get")]
public class GetWallet : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqGetCurrencyData req = await ReadData<ReqGetCurrencyData>();
        User user = GetUser();

        ResGetCurrencyData response = new();

        foreach (KeyValuePair<CurrencyType, long> currency in user.Currency)
        {
            response.Currency.Add(new NetUserCurrencyData() { Type = (int)currency.Key, Value = currency.Value });
        }

        await WriteDataAsync(response);
    }
}
