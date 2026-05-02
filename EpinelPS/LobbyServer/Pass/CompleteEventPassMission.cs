using EpinelPS.Database;

namespace EpinelPS.LobbyServer.Pass;

[GameRequest("/pass/event/completemission")]
public class CompleteEventPassMission : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqCompleteEventPassMission req = await ReadData<ReqCompleteEventPassMission>(); //fields "PassId", "PassMissionList"
        User user = GetUser();

        ResCompleteEventPassMission response = new(); // field Reward

        NetRewardData reward = new()
        {
            PassPoint = { }
        };

        PassHelper.CompletePassMissions(user, ref reward, req.PassId, [.. req.PassMissionList]);
        response.Reward = reward;

        JsonDb.Save();
        await WriteDataAsync(response);
    }
}