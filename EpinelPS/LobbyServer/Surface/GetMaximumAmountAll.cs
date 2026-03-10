using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Surface;

[PacketPath("/Surface/Export/MaxAmount/All")]
public class GetMaximumAmountAll : LobbyMsgHandler
{
    protected override async Task HandleAsync()
    {
        ReqListAllSurfaceCurrencyMaxAmount req = await ReadData<ReqListAllSurfaceCurrencyMaxAmount>();
        User user = User;

        ResListAllSurfaceCurrencyMaxAmount response = new();

        // TODO

        await WriteDataAsync(response);
    }
}
