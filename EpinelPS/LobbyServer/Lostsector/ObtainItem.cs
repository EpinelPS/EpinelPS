using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Lostsector
{
    [PacketPath("/lostsector/obtainitem")]
    public class ObtainItem : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqObtainLostSectorItem>();
            var user = GetUser();

            ResObtainLostSectorItem response = new();

            var lostSectorUser = user.LostSectorData[req.SectorId];
            lostSectorUser.ObtainedRewards++;

            if (lostSectorUser.Objects.ContainsKey(req.Object.PositionId))
                lostSectorUser.Objects[req.Object.PositionId] = req.Object;
            else
                lostSectorUser.Objects.Add(req.Object.PositionId, req.Object);


            // Get map info
            MapInfo map = GameData.Instance.MapData[GameData.Instance.LostSector[req.SectorId].field_id];

            // find reward
            var rewardEntry = map.ItemSpawner.Where(x => x.positionId == req.Object.PositionId).FirstOrDefault() ?? throw new Exception("cannot find reward");

            var positionReward = GameData.Instance.FieldItems[rewardEntry.itemId];
            response.Reward = RewardUtils.RegisterRewardsForUser(user, positionReward.type_value);

            if (positionReward.is_final_reward)
            {
                lostSectorUser.RecievedFinalReward = true;
            }

            JsonDb.Save();
            await WriteDataAsync(response);
        }
    }
}

