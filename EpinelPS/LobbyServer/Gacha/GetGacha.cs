using EpinelPS.Data;
using EpinelPS.Database;
namespace EpinelPS.LobbyServer.Gacha;

[GameRequest("/Gacha/Get")]
public class GetGacha : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqGetGachaData req = await ReadData<ReqGetGachaData>();
        User user = GetUser();

        ResGetGachaData response = new();

        // TODO: should not return anything when not completed chatper 2

        // Adding a default GachaType if the tutorial is done
        if (user.GetGachaCountForType( GachaPremiumType.GachaTutorial) > 0)
        {
            response.Gacha.Add(new NetUserGachaData() { GachaType = 3, PlayCount = 1 });
        }

        response.Gacha.Add(new NetUserGachaData() { GachaType = 9, PlayCount = 0 }); //type 9 = pickup gacha
        response.GachaEventData.Add(new NetGachaEvent() { FreeCount = 1, GachaTypeId = 9 });
        response.MultipleCustom.AddRange(user.CharacterWishlist.Select(id => new NetGachaCustomData() { Type = 9, Tid = id.CharacterId })); // Fill the user wishlist
        
        // Daily discount used
        if (user.DailyDiscountUsed)
            response.GachaDiscountData.Add(new NetUserGachaDiscountData() { GachaTypeId = 1, Count = 1 });  //type 1 = normal/wishlist gacha

        // TODO: response.GachaGuaranteedData

        // Selectup Lists
        var selectupList = GameData.Instance.GachaSelectupListTable.GroupBy(s => s.Value.GachaTypeId);

        // Add first entry or player selected entry if available
        foreach (var selectup in selectupList)
        {
            if (!user.GachaSelectupChoices.ContainsKey(selectup.Key))
            {
                user.GachaSelectupChoices.Add(selectup.Key, selectup.First().Value.Id);
                JsonDb.Save();
            }

            response.GachaSelectupData.Add(new NetUserGachaSelectupData()
            {
                GachaSelectupId = user.GachaSelectupChoices[selectup.Key], //e.g. 10303,
                GachaTypeId = selectup.Key // e.g. 10078
            });
        }

        // Write the response back
        await WriteDataAsync(response);
    }
}
