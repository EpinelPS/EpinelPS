using EpinelPS.Data;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Inventory;

[GameRequest("/inventory/getharmonycube")]
public class GetHarmonyCube : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqGetHarmonyCube req = await ReadData<ReqGetHarmonyCube>();
        User user = GetUser();

        ResGetHarmonyCube response = new();

        response.HarmonyCubes.AddRange(NetUtils.GetUserHarmonyCubes(user));


        await WriteDataAsync(response);
    }
}
