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

        // Tower Schedules: every tower is open all day, every day
        var allTimeSchedule = () => new NetSchedule { AllTime = new() };

        // Tower Data
        List<NetTowerData> towerData = [];
        foreach (var towerType in Enum.GetValues<CorporationTowerType>())
        {
            towers.TryGetValue(towerType, out int count);

            // Always open, and the clear counter is never consumed (always shows full)
            towerData.Add(towerType == CorporationTowerType.ALL
                ? new NetTowerData { Type = (int)towerType }
                : new NetTowerData { Type = (int)towerType, RemainCount = 3 });

            towerData.Last().Schedules.Add(allTimeSchedule());
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
