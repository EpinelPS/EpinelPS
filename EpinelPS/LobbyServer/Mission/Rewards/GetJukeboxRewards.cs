namespace EpinelPS.LobbyServer.Mission.Rewards;

[GameRequest("/mission/getrewarded/jukebox")]
public class GetJukeboxRewards : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqGetJukeboxRewardedData req = await ReadData<ReqGetJukeboxRewardedData>();
        User user = GetUser();

        // Tell the client which jukebox milestone rewards were already claimed
        ResGetJukeboxRewardedData response = new();
        response.JukeboxMissionTidList.AddRange(user.ClaimedJukeboxRewardTriggers);

        await WriteDataAsync(response);
    }
}
