
using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Utils;


namespace EpinelPS.LobbyServer.Gacha;

/// <summary>
/// This returns the status of the pity banner for the current user
/// </summary>
[GameRequest("/gacha/getpayback")]
public class GetPayback : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqGetGachaPaybackData req = await ReadData<ReqGetGachaPaybackData>();

        ResGetGachaPaybackData response = new();

        foreach(NetGachaPaybackData gpd in User.GachaPaybackData.Values){
             response.PaybackDataList.Add(gpd);            
        }
     
        await WriteDataAsync(response);
    }
}
