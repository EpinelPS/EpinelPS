using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Pass
{
    [PacketPath("/pass/getactive")]
    public class GetActivePassData : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqGetActivePassData>();
			
            var response = new ResGetActivePassData();
            response.PassExist = true; 
            response.Pass = new NetPassInfo { PassId = 1027, PassPoint = 490, PassSkipCount = 15, PremiumActive = true };
            
			// Adding PassRankList using a loop
            for (int rank = 1; rank <= 15; rank++)
            {
                response.Pass.PassRankList.Add(new NetPassRankData { PassRank = rank, IsNormalRewarded = true, IsPremiumRewarded = true });
            }

            var missionIds = new[] { 3001, 3002, 3003, 3004, 3005, 3006,3007 };
            foreach (var missionId in missionIds) response.Pass.PassMissionList.Add(new NetPassMissionData { PassMissionId = missionId, IsComplete = true });
           
		   await WriteDataAsync(response);
        }
    }
}
