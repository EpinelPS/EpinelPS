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
            response.PassExist = false; // if true game wont load but wont load pass either this is just broken
            response.Pass = new NetPassInfo { PassId = 1000020, PassPoint = 11016, PassSkipCount = 0, PremiumActive = true };
            response.Pass.PassRankList.Add(new NetPassRankData { PassRank = 3, IsNormalRewarded = false, IsPremiumRewarded = true });
            var missionIds = new[] { 2001701, 2001702, 2001703, 2001704, 2001705, 2001706 };
            foreach (var missionId in missionIds) response.Pass.PassMissionList.Add(new NetPassMissionData { PassMissionId = missionId, IsComplete = false });
           
		   await WriteDataAsync(response);
        }
    }
}
