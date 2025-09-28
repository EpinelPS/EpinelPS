using EpinelPS.Utils;
using EpinelPS.Data;

namespace EpinelPS.LobbyServer.Outpost
{
    [PacketPath("/infracore/check")]
    public class CheckInfracore : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqCheckReceiveInfraCoreReward req = await ReadData<ReqCheckReceiveInfraCoreReward>();
            ResCheckReceiveInfraCoreReward response = new();

            User user = GetUser();

            bool isReceived = false;
            
            int currentLevel = user.InfraCoreLvl;
            
            Dictionary<int, InfraCoreGradeRecord> gradeTable = GameData.Instance.InfracoreTable;
            if (gradeTable.TryGetValue(currentLevel, out var gradeData))
            {
                if (gradeData.RewardId > 0)
                {
                    isReceived = user.InfraCoreRewardReceived.ContainsKey(currentLevel) && user.InfraCoreRewardReceived[currentLevel];
                }
            }

            response.IsReceived = isReceived;

            await WriteDataAsync(response);
        }
    }
}
