using EpinelPS.Database;
using EpinelPS.Utils;
using EpinelPS.StaticInfo;

namespace EpinelPS.LobbyServer.Tower
{
    [PacketPath("/tower/cleartower")]
    public class ClearTower : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqClearTower>();

            var response = new ResClearTower();
            var user = GetUser();

            if (req.BattleResult == 1)
            {
                response = CompleteTower(user, req.TowerId);
            }

            await WriteDataAsync(response);
        }

        public static ResClearTower CompleteTower(User user, int TowerId)
        {
            var response = new ResClearTower();

            if (!GameData.Instance.towerTable.TryGetValue(TowerId, out TowerRecord? record)) throw new Exception("unable to find tower with id " + TowerId);

            // Parse TowerId to get TowerType and FloorNumber
            int TowerType = (TowerId / 10000) - 1; // For some weird reason the Type here doesn't match up with NetTowerData, thus the -1
            int FloorNumber = TowerId % 10000;

            // Update user's TowerProgress
            if (!user.TowerProgress.TryGetValue(TowerType, out int value))
            {
                user.TowerProgress[TowerType] = record.floor;
            }
            else if (value < FloorNumber)
            {
                user.TowerProgress[TowerType] = record.floor;
            }

            if (record.type == "TETRA")
            {
                user.AddTrigger(TriggerType.TowerTetraClear, TowerId);
            }
            else if (record.type == "ELYSION")
            {
                user.AddTrigger(TriggerType.TowerElysionClear, TowerId);
            }
            else if (record.type == "MISSILIS")
            {
                user.AddTrigger(TriggerType.TowerMissilisClear, TowerId);
            }
            else if (record.type == "PILGRIM")
            {
                user.AddTrigger(TriggerType.TowerOverspecClear, TowerId);
            }
            else if (record.type == "ALL")
            {
                user.AddTrigger(TriggerType.TowerBasicClear, TowerId);
            }

            var reward = GameData.Instance.GetRewardTableEntry(record.reward_id) ?? throw new Exception("failed to get reward");
            response.Reward = RewardUtils.RegisterRewardsForUser(user, reward);


            JsonDb.Save();

            return response;
        }
    }
}