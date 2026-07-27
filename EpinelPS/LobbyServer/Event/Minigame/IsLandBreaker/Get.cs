using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.LobbyServer.Minigame;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Event.Minigame.IsLandBreaker;

[GameRequest("/event/minigame/islandbreaker/get")]
public class Get : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqGetMiniGameIslandBreaker req = await ReadData<ReqGetMiniGameIslandBreaker>();
        User user = GetUser();
        ResGetMiniGameIslandBreaker response = new();

        var manage = GameData.Instance.EventIslandBreakerManagerTable.Values
            .Where(x => x.MinigameType == MiniGameSystemType.Normal).FirstOrDefault();

        if(!user.IsLandBreakerDatas.TryGetValue(req.IslandBreakerId,out var isLandData))
        {
            isLandData = new()
            {
                IslandBreakerId = req.IslandBreakerId
            };

            var missionlist = GameData.Instance.EventIslandBreakerMissionTable.Values
               .Where(x => x.MissionGroup == manage.MissionGroup).ToList();

            foreach (var item in missionlist)
            {
                isLandData.Missions.TryAdd(item.Id, new()
                {
                    Rewarded = false,
                    MissionId = item.Id,
                    Progress = 0
                });
            }

            var chardata = GameData.Instance.EventIslandBreakerCharacterTable.Values.ToList();
            foreach (var item in chardata)
            {
                isLandData.CharacterStatistics.TryAdd(item.Id, new()
                {
                    CharacterId = item.Id                    
                });
            }

            isLandData.LastSelectedCharacterId = chardata[0].Id;

            var bufflist = GameData.Instance.EventIslandBreakerBuffTable.Values
                .Where(x => x.BuffLevel == 0)
                .Select(x=>x.Id)
                .ToList();
            isLandData.Buffs.AddRange(bufflist);


            var currencylist = GameData.Instance.EventIslandBreakerCurrencyTable.Values.ToList();
            foreach (var item in currencylist)
            {
                isLandData.Currencies.TryAdd(item.Id, new()
                {
                    CurrencyId = item.Id,
                    MaxLimit = item.Limit3Max,
                    CurrentAmount = 0
                });
            }


            user.IsLandBreakerDatas[req.IslandBreakerId] = isLandData;
        }

        int day = user.GetDateDay();
        if (day != isLandData.ActiveDate) 
        {
            isLandData.ActiveDate = day;
            isLandData.DailyScore = new() { IsDailyRewarded = false, Score = 0 };
        }

        response = isLandData.ToNet();

        // TODO
        JsonDb.Save();
        await WriteDataAsync(response);
    }
}