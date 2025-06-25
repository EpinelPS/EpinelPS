using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Lostsector
{
    [PacketPath("/lostsector/perfectreward")]
    public class GetPerfectReward : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqLostSectorPerfectReward>();
            var user = GetUser();

            var response = new ResLostSectorPerfectReward();

            if (user.LostSectorData.ContainsKey(req.SectorId))
            {
                var lostSectorData = user.LostSectorData[req.SectorId];
                lostSectorData.CompletedPerfectly = true;

                var sectorInfo = GameData.Instance.LostSector[req.SectorId];

                response.Reward = RewardUtils.RegisterRewardsForUser(user, sectorInfo.exploration_reward);
            }

            JsonDb.Save();
            await WriteDataAsync(response);
        }
    }
}

