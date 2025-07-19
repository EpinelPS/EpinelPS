using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Character.Counsel;

[PacketPath("/character/attractive/counsel")]
public class DoCounsel : LobbyMsgHandler
{
    protected override async Task HandleAsync()
    {
        var req = await ReadData<ReqCharacterCounsel>();
        var user = GetUser();

        ResCharacterCounsel response = new();

        foreach (var currency in user.Currency)
        {
            response.Currencies.Add(new NetUserCurrencyData() { Type = (int)currency.Key, Value = currency.Value });
        }

        var currentBondInfo = user.BondInfo.Where(x => x.NameCode == req.NameCode);

        NetUserAttractiveData data;

        if (currentBondInfo.Any())
        {
            data = currentBondInfo.First();

            // TODO update
            response.Attractive = data;
        }
        else
        {
            data = new()
            {
                NameCode = req.NameCode,
                // TODO
            };

            response.Attractive = data;
            user.BondInfo.Add(data);
        }

        JsonDb.Save();

        // TODO: Validate response from real server and pull info from user info
        await WriteDataAsync(response);
    }
}
