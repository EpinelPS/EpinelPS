using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Utils;
using Google.Protobuf;

namespace EpinelPS.LobbyServer.Minigame.TTS;

[GameRequest("/MiniGame/TTS/Skin/Buy")]
public class SkinBuy : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqBuyMiniGameTtsSkin req = await ReadData<ReqBuyMiniGameTtsSkin>();
        User user = GetUser();
        ResBuyMiniGameTtsSkin response = new();
        if (user.TTSGameData.TryGetValue(req.EventTtsManagerTableId, out var ttsData))
        {
            EventTTSSkinObjectRecord_Raw? skinobj = GameData.Instance.EventTTSSkinObjectTable.Values
                .Where(x => x.Id == req.EventTtsSkinObjectId).FirstOrDefault();
            bool resault = user.SubtractCurrency((CurrencyType)skinobj.CostCurrencyId, skinobj.CostCurrencyValue);
            if (resault)
            {
                ttsData.BuySkinObject.Add(skinobj.Id);
            }

            foreach (KeyValuePair<CurrencyType, long> item in user.Currency)
            {
                response.Currency.Add(new NetUserCurrencyData() { Type = (int)item.Key, Value = item.Value });
            }

            response.EventTtsSkinObjectIdList.AddRange(ttsData.BuySkinObject);
        }
        
        // TODO
        JsonDb.Save();
        await WriteDataAsync(response);
    }
}