using EpinelPS.Data;

namespace EpinelPS.LobbyServer.Gacha;

[GameRequest("/gacha/pity/list")]
public class ListPity : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqListGachaPityProgress req = await ReadData<ReqListGachaPityProgress>();

        ResListGachaPityProgress response = new();

        User user = GetUser();

        // The only pity banner in GachaPityTable is the Bonus Recruit. If more are added later, this will need to be modified. 
        var pityBanner = GameData.Instance.GachaPityRecords.First().Value;
        
        response.GachaPityProgress.Add(new NetGachaPityData()
        {
            // Bonus Recruit Pity Banner ID
            GachaPityId = pityBanner.Id,     
            
            // Amount of character already obtained
            PityExecuteCount = user.GachaPityBannerExecuteCount.Where(b => b.Key == pityBanner.Id).Sum(b => b.Value),

            // Total amount of pulls done on standard banner (no need to calculate anything)
            TotalGachaCount = user.GetGachaCountForType(GachaPremiumType.GachaPremium),

            // Total amount of pulls "used" (100 * PityExecuteCount). 
            TotalUsedGachaCount = user.GachaPityBannerExecuteCount.Where(b => b.Key == pityBanner.Id).Sum(b => b.Value) * pityBanner.NeedGachaCount
        });

        await WriteDataAsync(response);
    }
}
