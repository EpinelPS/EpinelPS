using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Utils;
using Org.BouncyCastle.Ocsp;

namespace EpinelPS.LobbyServer.Tower;

[GameRequest("/tower/cleartower")]
public class ClearTower : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqClearTower req = await ReadData<ReqClearTower>();

        ResClearTower response = new();
        User user = GetUser();

            //Console.WriteLine($"[ClearTower] 战役结果 : {req.BattleResult}");
            

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

            //Console.WriteLine($"[ClearTower] 通关塔ID : {record.Id} , 类型：{record.Type} , 层：{record.Floor}");

            // Parse TowerId to get TowerType and FloorNumber
            //int TowerType = (TowerId / 10000) - 1; // For some weird reason the Type here doesn't match up with NetTowerData, thus the -1
            //int FloorNumber = TowerId % 10000;
            int TowerType = (int)record.Type;
            int FloorNumber = record.Floor;


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
            user.ResetableData.TowerCount[3] += 1;
            user.AddTrigger(Trigger.TowerTetraClear, 1, TowerId);
        }
        else if (record.Type == CorporationTowerType.ELYSION)
        {
            user.ResetableData.TowerCount[1] += 1;
            user.AddTrigger(Trigger.TowerElysionClear, 1, TowerId);
        }
        else if (record.Type == CorporationTowerType.MISSILIS)
        {
            user.ResetableData.TowerCount[2] += 1;
            user.AddTrigger(Trigger.TowerMissilisClear, 1, TowerId);
        }
        else if (record.Type == CorporationTowerType.OVERSPEC)
        {
            user.ResetableData.TowerCount[4] += 1;
            user.AddTrigger(Trigger.TowerOverspecClear, 1, TowerId);
        }
        else if (record.Type == CorporationTowerType.ALL)
        {

            user.AddTrigger(Trigger.TowerBasicClear, 1, TowerId);
        }

            RewardRecord reward = GameData.Instance.GetRewardTableEntry(record.RewardId) ?? throw new Exception("failed to get reward");

            //Console.WriteLine($"[ClearTower] 奖励ID : {record.RewardId}");

            response.Reward = RewardUtils.RegisterRewardsForUser(user, reward);


        JsonDb.Save();

        return response;
    }
}