using EpinelPS.Database;
using EpinelPS.Utils;
using EpinelPS.Data;

namespace EpinelPS.LobbyServer.Tower
{
    [PacketPath("/tower/cleartower")]
    public class ClearTower : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqClearTower req = await ReadData<ReqClearTower>();

            ResClearTower response = new();
            User user = GetUser();

            if (req.BattleResult == 1)
            {
                response = CompleteTower(user, req.TowerId);
            }

            await WriteDataAsync(response);
        }

        public static ResClearTower CompleteTower(User user, int TowerId)
        {
            ResClearTower response = new();

            if (!GameData.Instance.towerTable.TryGetValue(TowerId, out TowerRecord? record)) throw new Exception("unable to find tower with Id " + TowerId);

            // Parse TowerId to get TowerType and FloorNumber
            int TowerType = (TowerId / 10000) - 1; // For some weird reason the Type here doesn't match up with NetTowerData, thus the -1
            int FloorNumber = TowerId % 10000;

            // Update user's TowerProgress
            if (!user.TowerProgress.TryGetValue(TowerType, out int value))
            {
                user.TowerProgress[TowerType] = record.Floor;
            }
            else if (value < FloorNumber)
            {
                user.TowerProgress[TowerType] = record.Floor;
            }

            if (record.Type == CorporationTowerType.TETRA)
            {
                user.AddTrigger(Trigger.TowerTetraClear, TowerId);
            }
            else if (record.Type == CorporationTowerType.ELYSION)
            {
                user.AddTrigger(Trigger.TowerElysionClear, TowerId);
            }
            else if (record.Type== CorporationTowerType.MISSILIS)
            {
                user.AddTrigger(Trigger.TowerMissilisClear, TowerId);
            }
            else if (record.Type == CorporationTowerType.OVERSPEC)
            {
                user.AddTrigger(Trigger.TowerOverspecClear, TowerId);
            }
            else if (record.Type == CorporationTowerType.ALL)
            {
                user.AddTrigger(Trigger.TowerBasicClear, TowerId);
            }

            RewardRecord reward = GameData.Instance.GetRewardTableEntry(record.RewardId) ?? throw new Exception("failed to get reward");
            response.Reward = RewardUtils.RegisterRewardsForUser(user, reward);


            JsonDb.Save();

            return response;
        }
    }
}