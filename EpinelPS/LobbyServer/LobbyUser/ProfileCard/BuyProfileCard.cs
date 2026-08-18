using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.LobbyUser.ProfileCard;

[GameRequest("/ProfileCard/Buy")]
public class BuyProfileCard : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        var req = await ReadData<ReqBuyProfileCardObject>();
        var user = GetUser();

        var response = new ResBuyProfileCardObject();
        GameData.Instance.ProfileCardObjectTable.TryGetValue(req.ObjectTid, out var card);
        user.ProfileCardsData.Add(card.Id);
        GameData.Instance.itemMaterialTable.TryGetValue(card.RequireItemId, out var requiredItem);
        var requiredItemInInventory = user.Items.Where(x => x.ItemType == requiredItem.Id).FirstOrDefault();
        requiredItemInInventory.Count -= card.RequireItemValue;
        response.ProfileCardTicketMaterialSync = NetUtils.UserItemDataToNet(requiredItemInInventory);

        JsonDb.Save();
        await WriteDataAsync(response);
    }
}
