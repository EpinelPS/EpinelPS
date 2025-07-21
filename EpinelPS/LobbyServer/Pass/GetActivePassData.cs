using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Pass
{
    [PacketPath("/pass/getactive")]
    public class GetActivePassData : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqGetActivePassData req = await ReadData<ReqGetActivePassData>();

            ResGetActivePassData response = new()
            {
                PassExist = true,
                Pass = new NetPassInfo { PassId = 1028, PassPoint = 490, PassSkipCount = 15, PremiumActive = true }
            };

            // Adding PassRankList using a loop
            for (int rank = 1; rank <= 15; rank++)
            {
                response.Pass.PassRankList.Add(new NetPassRankData { PassRank = rank, IsNormalRewarded = true, IsPremiumRewarded = true });
            }

            int[] missionIds = new[] { 4001, 4002, 4003, 4004, 4005, 4006, 4007 };
            foreach (int missionId in missionIds) response.Pass.PassMissionList.Add(new NetPassMissionData { PassMissionId = missionId, IsComplete = true });
           
		   await WriteDataAsync(response);
        }
    }
}
