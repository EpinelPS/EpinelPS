using System.Text.Json;
using EpinelPS.Data;

namespace EpinelPS.LobbyServer.Tower;

[GameRequest("/tower/gettowerdata")]
public class GetTowerData : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqGetTowerData req = await ReadData<ReqGetTowerData>();
        ResGetTowerData response = new();
        User user = GetUser();

        user.ResetableData ??= new();
        user.ResetableData.TowerCount ??= Enum.GetValues<CorporationTowerType>().ToDictionary(t => t, _ => 0);

        Dictionary<CorporationTowerType, int> towers = user.ResetableData.TowerCount;
        if (towers.Count == 0)
        {
            towers = Enum.GetValues<CorporationTowerType>()
                .ToDictionary(t => t, _ => 0);
            user.ResetableData.TowerCount = towers;
        }

        // Tower Schedules
        var towerSchedules = new Dictionary<CorporationTowerType, NetSchedule>
        {
            [CorporationTowerType.ELYSION] = new() { DayOfWeek = new() { DayOfWeeks = { 1, 4, 6 }, StartTime = 720000000000, Duration = 863990000000 } },
            [CorporationTowerType.MISSILIS] = new() { DayOfWeek = new() { DayOfWeeks = { 2, 5, 6 }, StartTime = 720000000000, Duration = 863990000000 } },
            [CorporationTowerType.TETRA] = new() { DayOfWeek = new() { DayOfWeeks = { 0, 3, 6 }, StartTime = 720000000000, Duration = 863990000000 } },
            [CorporationTowerType.OVERSPEC] = new() { DayOfWeek = new() { DayOfWeeks = { 2, 6 }, StartTime = 720000000000, Duration = 863990000000 } },
            [CorporationTowerType.ALL] = new() { AllTime = new() }
        };

        // Tower Data
        List<NetTowerData> towerData = [];
        foreach (var towerType in Enum.GetValues<CorporationTowerType>())
        {
            towers.TryGetValue(towerType, out int count);

            towerData.Add(towerType == CorporationTowerType.ALL
                ? new NetTowerData { Type = (int)towerType }
                : new NetTowerData { Type = (int)towerType, RemainCount = Math.Max(0, 3 - count) });

            towerData.Last().Schedules.Add(towerSchedules[towerType]);
        }

        if (user.TowerProgress.ContainsKey(0)) user.TowerProgress.Remove(0);
        foreach (var towerProgress in user.TowerProgress)
        {
            towerData.FirstOrDefault(x => x.Type == ((int)towerProgress.Key))?.Floor = towerProgress.Value;
        }
        System.Console.WriteLine(JsonSerializer.Serialize(towerData));
        response.Data.AddRange(towerData);

        await WriteDataAsync(response);
    }
}
