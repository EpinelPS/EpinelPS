namespace EpinelPS.LobbyServer.Liberate;

[GameRequest("/liberate/getprogresslist")]
public class GetProgressList : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqGetLiberateProgressList req = await ReadData<ReqGetLiberateProgressList>();
        User user = GetUser();

        ResGetLiberateProgressList response = new();
        foreach (var item in user.LiberateDatas)
        {
            NetLiberateProgressData netLiberate = new NetLiberateProgressData
            {
                CharacterId = item.Value.CharacterId,
                IsCompleted = item.Value.IsCompleted,
                ProgressPoint = item.Value.ProgressPoint,
                RewardedCount = item.Value.RewardedCount,
                Step = item.Value.Step
            };
            response.LiberateProgressData.Add(netLiberate);
        }

        await WriteDataAsync(response);
    }
}