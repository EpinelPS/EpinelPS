using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using EpinelPS.Data;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Tower;

public class TowerHelper
{
    private static readonly Dictionary<CorporationTowerType, Trigger> TowerClearTriggers = new()
    {
        [CorporationTowerType.ELYSION] = Trigger.TowerElysionClear,
        [CorporationTowerType.MISSILIS] = Trigger.TowerMissilisClear,
        [CorporationTowerType.TETRA] = Trigger.TowerTetraClear,
        [CorporationTowerType.OVERSPEC] = Trigger.TowerOverspecClear,
        [CorporationTowerType.ALL] = Trigger.TowerBasicClear,
    };

    private static int GetMaxFloor(CorporationTowerType type) => GameData.Instance.towerTable.Values
            .Where(t => t.Type == type)
            .Select(t => t.Floor)
            .DefaultIfEmpty(0)
            .Max();
    
    public static NetRewardData CompleteTower(User user, int towerId)
    {
        if (!GameData.Instance.towerTable.TryGetValue(towerId, out TowerRecord? record))
            throw new Exception("unable to find tower with Id " + towerId);

        int maxFloor = GetMaxFloor(record.Type);

        if (record.Floor < 1 || record.Floor > maxFloor)
            throw new Exception($"invalid floor {record.Floor} for {record.Type} (max={maxFloor})");

        if (user.TowerProgress.TryGetValue(record.Type, out int progress) && progress >= maxFloor)
            throw new Exception($"tower {record.Type} already fully cleared (progress={progress}, max={maxFloor})");

        if (user.Triggers.Any(x => x.ConditionId == towerId && x.Value == 1))
            throw new Exception("floor already cleared");

        // Sequential progression: previous floor must be cleared (unless this is floor 1)
        if (record.Floor > 1)
        {
            var prev = GameData.Instance.towerTable.Values
                .FirstOrDefault(t => t.Type == record.Type && t.Floor == record.Floor - 1);

            if (prev != null && !user.Triggers.Any(x => x.ConditionId == prev.Id && x.Value == 1))
                throw new Exception($"previous floor {record.Floor - 1} not cleared");
        }

        if (!user.TowerProgress.TryGetValue(record.Type, out int value) || value < record.Floor)
            user.TowerProgress[record.Type] = record.Floor;

        if (record.Type is not CorporationTowerType.ALL)
            user.ResetableData.TowerCount[record.Type] += 1;

        user.AddTrigger(TowerClearTriggers[record.Type], 1, towerId);

        RewardRecord rewardRecord = GameData.Instance.GetRewardTableEntry(record.RewardId)
            ?? throw new Exception("failed to get reward");

        NetRewardData reward = RewardUtils.RegisterRewardsForUser(user, rewardRecord);

        Console.WriteLine($"Completed floor {record.Floor}");
        return reward;
    }

    public static NetRewardData SkipTowerFloors(User user, int towerId, out List<(Trigger Type, int Value, int ConditionId)> triggersToAdd)
    {
        if (!GameData.Instance.towerTable.TryGetValue(towerId, out TowerRecord? record))
            throw new Exception("unable to find tower with Id " + towerId);

        int maxFloor = GetMaxFloor(record.Type);

        if (record.Floor < 1 || record.Floor > maxFloor)
            throw new Exception($"invalid floor {record.Floor} for {record.Type} (max={maxFloor})");

        user.TowerProgress.TryGetValue(record.Type, out int progress);

        if (progress >= maxFloor)
            throw new Exception($"tower {record.Type} already fully cleared (progress={progress}, max={maxFloor})");

        if (user.Triggers.Any(x => x.ConditionId == towerId && x.Value == 1))
            throw new Exception("floor already cleared");

        NetRewardData totalReward = new NetRewardData();
        int totalFloorsCleared = 0;
        triggersToAdd = new List<(Trigger, int, int)>();

        int startFloor = progress + 1;

        var floorsToSkip = GameData.Instance.towerTable.Values
            .Where(t => t.Type == record.Type
                && t.Floor >= startFloor
                && t.Floor <= record.Floor
                && !user.Triggers.Any(x => x.ConditionId == t.Id && x.Value == 1))
            .OrderBy(t => t.Floor)
            .ToList();

        if (floorsToSkip.Count != record.Floor - progress)
            throw new Exception($"tower table has gaps for {record.Type} between {startFloor} and {record.Floor}");

        Console.WriteLine($"Skipping mode - Completing floors {startFloor} to {record.Floor}");

        foreach (var floor in floorsToSkip)
        {
            RewardRecord floorReward = GameData.Instance.GetRewardTableEntry(floor.RewardId)
                ?? throw new Exception($"failed to get reward for floor {floor.Floor}");

            var reward = RewardUtils.RegisterRewardsForUser(user, floorReward);

            totalReward.UserItems.AddRange(reward.UserItems);
            totalReward.Currency.AddRange(reward.Currency);

            triggersToAdd.Add((TowerClearTriggers[floor.Type], 1, floor.Id));

            totalFloorsCleared++;
            Console.WriteLine($"Completed floor {floor.Floor} - Reward added");
        }

        user.TowerProgress[record.Type] = record.Floor;

        if (record.Type is not CorporationTowerType.ALL)
            user.ResetableData.TowerCount[record.Type] += totalFloorsCleared;

        Console.WriteLine($"Total floors cleared: {totalFloorsCleared}");

        foreach (var currency in totalReward.Currency.GroupBy(c => c.Type))
            Console.WriteLine($"Currency {currency.Key}: {currency.Sum(c => c.Value)}");

        return totalReward;
    }
}