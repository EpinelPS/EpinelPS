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
            ReqObtainLostSectorItem req = await ReadData<ReqObtainLostSectorItem>();
            User user = GetUser();

            ResObtainLostSectorItem response = new();

            LostSectorData lostSectorUser = user.LostSectorData[req.SectorId];
            lostSectorUser.ObtainedRewards++;

            if (lostSectorUser.Objects.ContainsKey(req.Object.PositionId))
                lostSectorUser.Objects[req.Object.PositionId] = req.Object;
            else
                lostSectorUser.Objects.Add(req.Object.PositionId, req.Object);


            // Get map info
            var map = GameData.Instance.MapData[GameData.Instance.LostSector[req.SectorId].FieldId];

            // find reward
            var rewardEntry = map.ItemSpawner.Where(x => x.PositionId == req.Object.PositionId).FirstOrDefault() ?? throw new Exception("cannot find reward");

            FieldItemRecord positionReward = GameData.Instance.FieldItems[rewardEntry.ItemId];
            response.Reward = RewardUtils.RegisterRewardsForUser(user, positionReward.TypeValue);
            response.BoxCount = lostSectorUser.ObtainedRewards;

            if (positionReward.IsFinalReward)
            {
                lostSectorUser.RecievedFinalReward = true;
            }

            JsonDb.Save();
            await WriteDataAsync(response);
        }
    }
}

