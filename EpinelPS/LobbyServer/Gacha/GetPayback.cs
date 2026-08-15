
using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Utils;


namespace EpinelPS.LobbyServer.Gacha;

/// <summary>
/// This returns the status of the payback banners for the current user
/// </summary>
[GameRequest("/gacha/getpayback")]
public class GetPayback : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqGetGachaPaybackData req = await ReadData<ReqGetGachaPaybackData>();
        var user = GetUser();

        ResGetGachaPaybackData response = new();

        foreach(GachaPaybackData gpd in user.GachaPaybackData.Values){

            var ngpd = new NetGachaPaybackData()
            {
                GachaId = gpd.GachaId,
                GachaCount = gpd.GachaCount
            };
            if (gpd.RewardedStepList.Count > 0)
                ngpd.RewardedStepList.AddRange(gpd.RewardedStepList);

             response.PaybackDataList.Add(ngpd);            
        }
     
        await WriteDataAsync(response);
    }
}
