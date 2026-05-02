namespace EpinelPS.LobbyServer.Soloraid;

[GameRequest("/soloraid/getperiod")]
public class GetPeriod : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        await ReadData<ReqGetSoloRaidPeriod>();

        ResGetSoloRaidPeriod response = new()
        {
            Period = SoloRaidHelper.GetSoloRaidPeriod()
        };
        // TODO
        await WriteDataAsync(response);
    }
}
