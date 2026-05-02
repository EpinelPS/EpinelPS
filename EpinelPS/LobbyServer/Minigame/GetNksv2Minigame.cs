namespace EpinelPS.LobbyServer.Minigame;

[GameRequest("/minigame/nksv2/get")]
public class GetNksv2Minigame : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqGetMiniGameNKSV2Data req = await ReadData<ReqGetMiniGameNKSV2Data>();

        ResGetMiniGameNKSV2Data response = new();
        // TODO
        await WriteDataAsync(response);
    }
}
