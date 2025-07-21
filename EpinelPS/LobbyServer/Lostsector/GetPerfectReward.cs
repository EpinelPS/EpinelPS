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
            ReqLostSectorPerfectReward req = await ReadData<ReqLostSectorPerfectReward>();
            User user = GetUser();

            ResLostSectorPerfectReward response = new();

            if (user.LostSectorData.TryGetValue(req.SectorId, out LostSectorData? lostSectorData))
            {
                lostSectorData.CompletedPerfectly = true;

                LostSectorRecord sectorInfo = GameData.Instance.LostSector[req.SectorId];

                response.Reward = RewardUtils.RegisterRewardsForUser(user, sectorInfo.exploration_reward);
            }

            JsonDb.Save();
            await WriteDataAsync(response);
        }
    }
}

