
using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Utils;
using System.Text.Json;

namespace EpinelPS.LobbyServer.Inventory.ProfileCard;

[GameRequest("/ProfileCard/ProfileRandomBox/Open")]
public class OpenProfileRandomBox : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        var req = await ReadData<ReqOpenProfileRandomBox>();
        var user = GetUser();
        var response = new ResOpenProfileRandomBox();

        DbItemData box = user.Items.Where(x => x.Isn == req.Isn).FirstOrDefault()
            ?? throw new InvalidDataException("cannot find box with isn " + req.Isn);
        GameData.Instance.ConsumableItems.TryGetValue(box.ItemType, out var item);
        var pcoTable = GameData.Instance.ProfileCardObjectTable.Values.ToList();

        if (req.NumOpens * item.UseFragCost > box.Count) throw new Exception("count mismatch");

        box.Count -= req.NumOpens * item.UseFragCost;
        if (box.Count == 0) user.Items.Remove(box);

        var randomReward = NetUtils.UseLootBox(user, box.ItemType, req.NumOpens);
        var userOwnedCards = user.ProfileCardsData;
        randomReward.ProfileCardObjects.ToList().ForEach(card =>
        {
            if (userOwnedCards.Contains(card))
            {
                GameData.Instance.itemMaterialTable.TryGetValue(pcoTable.FirstOrDefault(c => c.Id == card).ExchangeItemId, out var exchangeItem);
                var rewardCount = pcoTable.FirstOrDefault(c => c.Id == card).ExchangeItemValue;
                var ret = new NetRewardData();
                RewardUtils.AddSingleObject(user, ref ret, exchangeItem.Id, RewardType.Item, rewardCount);
                response.ProfileCardTicketMaterialSync.Add(ret.UserItems.First());
                response.OpeningResult.Add(new ProfileRandomBoxSingleOpeningResult
                {
                    ObjectTid = card,
                    ExchangedForTicketMaterial = true,
                });
            }
            else
            {
                userOwnedCards.Add(card);
                response.OpeningResult.Add(new ProfileRandomBoxSingleOpeningResult
                {
                    ObjectTid = card,
                    ExchangedForTicketMaterial = false,
                });
            }
        });
        response.ProfileCardTicketMaterialSync.Add(NetUtils.UserItemDataToNet(box));
        JsonDb.Save();
        await WriteDataAsync(response);
    }
}