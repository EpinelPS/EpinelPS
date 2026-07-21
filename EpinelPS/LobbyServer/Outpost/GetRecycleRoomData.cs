namespace EpinelPS.LobbyServer.Outpost;

[GameRequest("/outpost/recycleroom/get")]
public class GetRecycleRoomData : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqGetRecycleRoomData req = await ReadData<ReqGetRecycleRoomData>();
        User user = GetUser();
        ResGetRecycleRoomData response = new();

        response.Recycle.AddRange(user.ResearchProgress.Select(progress =>
        {
            return new NetUserRecycleRoomData()
            {
                Tid = progress.Key,
                Lv = progress.Value.Level
            };
        }));

        await WriteDataAsync(response);
    }
}
