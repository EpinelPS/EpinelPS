using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Pass
{
    [PacketPath("/pass/completemission")]
    public class CompletePassMission : LobbyMsgHandler
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
}