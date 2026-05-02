namespace EpinelPS.LobbyServer.Pass;

[GameRequest("/pass/event/freereward/getactive")]
public class GetActiveFreeRewardData : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqGetActiveFreeRewardPassData req = await ReadData<ReqGetActiveFreeRewardPassData>();
        ResGetActiveFreeRewardPassData response = new();

        await WriteDataAsync(response);
    }
}
