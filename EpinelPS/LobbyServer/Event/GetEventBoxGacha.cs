using EpinelPS.Models;

namespace EpinelPS.LobbyServer.Event;

[GameRequest("/event/boxgacha/get")]
public class GetEventBoxGacha : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqGetEventBoxGacha req = await ReadData<ReqGetEventBoxGacha>();
        User user = GetUser();

        ResGetEventBoxGacha response = new();

        if (user.EventBoxGachaData.TryGetValue(req.EventId, out var gachaData))
        {
            response.GachaCount = gachaData.GachaCount;
            response.RewardOrders.AddRange(gachaData.RewardOrders);
        }

        await WriteDataAsync(response);
    }
}