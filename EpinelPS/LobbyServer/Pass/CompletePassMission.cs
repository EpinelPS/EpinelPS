using EpinelPS.Database;

namespace EpinelPS.LobbyServer.Pass;

[GameRequest("/pass/completemission")]
public class CompletePassMission : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqCompletePassMission req = await ReadData<ReqCompletePassMission>(); //fields "PassId", "PassMissionList"
        User user = GetUser();

        ResCompletePassMission response = new(); // field Reward

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