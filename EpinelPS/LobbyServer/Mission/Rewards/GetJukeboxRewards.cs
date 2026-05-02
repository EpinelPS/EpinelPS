namespace EpinelPS.LobbyServer.Mission.Rewards;

[GameRequest("/mission/getrewarded/jukebox")]
public class GetJukeboxRewards : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqGetJukeboxRewardedData req = await ReadData<ReqGetJukeboxRewardedData>();

        // TODO: save these things
        ResGetJukeboxRewardedData response = new();

        await WriteDataAsync(response);
    }
}
