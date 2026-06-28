namespace EpinelPS.LobbyServer.Shop.InApp;

[GameRequest("/inappshop/jupiter/getorderdetail")]
public class GetJupiterOrderDetail : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqGetJupiterOrderDetail req = await ReadData<ReqGetJupiterOrderDetail>();

        ResGetJupiterOrderDetail response = new()
        {
            Status = "SUCCESS"
        };

        await WriteDataAsync(response);
    }
}
