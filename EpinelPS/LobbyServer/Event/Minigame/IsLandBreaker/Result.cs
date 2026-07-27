using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.LobbyServer.Minigame;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Event.Minigame.IsLandBreaker;

[GameRequest("/event/minigame/islandbreaker/result")]
public class Result : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqMiniGameIslandBreakerResult req = await ReadData<ReqMiniGameIslandBreakerResult>();
        User user = GetUser();
        ResMiniGameIslandBreakerResult response = new();
        NetRewardData ret = new();

        var manage = GameData.Instance.EventIslandBreakerManagerTable.Values
            .Where(x => x.MinigameType == MiniGameSystemType.Normal).FirstOrDefault();
        if (user.IsLandBreakerDatas.TryGetValue(req.IslandBreakerId, out var isLandData))
        {
            isLandData.CharacterStatistics[req.CharacterId].CumulativeScore += req.Score;
            isLandData.CharacterStatistics[req.CharacterId].PlayCount += 1;
            isLandData.CumulativePlayScore += req.Score;
            isLandData.DailyScore.Score += req.Score;

            if (isLandData.DailyScore.IsDailyRewarded==false)
            {
                ret = RewardUtils.RegisterRewardsForUser(user, manage.DailyRewardId);
                response.DailyReward = ret;
                isLandData.DailyScore.IsDailyRewarded = true;
            }

            if (req.Score > isLandData.HighScore.HighScore)
            {
                isLandData.HighScore.HighScore = req.Score;
                MiniGameHelper.InsertOrUpdate(manage.EventId, user.ID, user.Guild.guildId.Value, req.Score, req.IslandBreakerId);
            }

            if (req.Wave > isLandData.HighScore.HighWave)
            {
                isLandData.HighScore.HighWave = req.Wave;
            }

            int currency = req.Wave <= 50 ? req.Wave * 2 : 100;

            isLandData.Currencies[2].CurrentAmount = Math.Min(
                isLandData.Currencies[2].CurrentAmount + currency,
                isLandData.Currencies[2].MaxLimit
            );

            isLandData.Currencies[2].Granted = currency;
            isLandData.Currencies[2].CumulativeAcquired += currency;

            var imageid = GameData.Instance.EventIslandBreakerImageTable.Values
                .Where(x => x.ImageGroup == manage.ImageGroup
                && x.ConditionCharacter == req.CharacterId
                && x.ConditionWave <= req.Wave)
                .OrderByDescending(x => x.ConditionWave)
                .FirstOrDefault();

            isLandData.Album.TryAdd(imageid.Id, new() { ImageId = imageid.Id, Unlocked = true });

            isLandData.CumulativeSummonBallCount += req.SummonBallCount;
            UpMission(isLandData, waveCount: req.Wave, req.CharacterId,req.Wave);
            response.BanResult = MiniGameBanResult.Success;
            response.Currencies.AddRange(MiniGameHelper
                .ToProtoDict<int, NetMiniGameIslandBreakerCurrency, MiniGameIslandBreakerCurrency>(isLandData.Currencies)
                .Values);

            response.DailyScore = isLandData.DailyScore.ToNet();
            response.HighScore = isLandData.HighScore.ToNet();

            response.UnlockedImage = isLandData.Album.Count;
            
        }

        // TODO
        JsonDb.Save();
        await WriteDataAsync(response);
    }

    public static void UpMission(IsLandBreakerData isLandData,int waveCount = 0,int charid = 0,int getCurrency = 0)
    {
        var manage = GameData.Instance.EventIslandBreakerManagerTable.Values
           .Where(x => x.Id == isLandData.IslandBreakerId).FirstOrDefault();

        List<EventIslandBreakerMissionRecord_Raw>? missionlist = GameData.Instance.EventIslandBreakerMissionTable.Values
               .Where(x => x.MissionGroup == manage.MissionGroup).ToList();


        foreach (IslandBreakerMissionType missionType in Enum.GetValues(typeof(IslandBreakerMissionType)))
        {
            switch (missionType)
            {               
                case IslandBreakerMissionType.WaveCount:
                    var wclist = missionlist
                        .Where(x => x.ConditionType == missionType).ToList();

                    foreach (var item in wclist)
                    {
                        if (isLandData.Missions.TryGetValue(item.Id, out var vaule))
                        {
                            vaule.Progress = waveCount;
                        }
                    }
                    break;
                case IslandBreakerMissionType.PlayPoint:
                    var pplist = missionlist
                        .Where(x => x.ConditionType == missionType).ToList();

                    foreach (var item in pplist)
                    {
                        if (isLandData.Missions.TryGetValue(item.Id, out var vaule))
                        {
                            vaule.Progress = isLandData.CumulativePlayScore;
                        }
                    }
                    break;
                case IslandBreakerMissionType.PlayCharacterPoint:
                    var pcplist = missionlist
                        .Where(x => x.ConditionType == missionType && x.ConditionId == charid).ToList();

                    foreach (var item in pcplist)
                    {
                        if (isLandData.Missions.TryGetValue(item.Id, out var vaule))
                        {
                            vaule.Progress = isLandData.CharacterStatistics[charid].CumulativeScore;                           
                        }
                    }

                    break;
                case IslandBreakerMissionType.SummonBall:
                    var sblist = missionlist
                        .Where(x => x.ConditionType == missionType).ToList();

                    foreach (var item in sblist)
                    {
                        if (isLandData.Missions.TryGetValue(item.Id, out var vaule))
                        {
                            vaule.Progress = isLandData.CumulativeSummonBallCount;
                        }
                    }
                    break;
                case IslandBreakerMissionType.GetCurrency:
                    var gclist = missionlist
                        .Where(x => x.ConditionType == missionType).ToList();

                    foreach (var item in gclist)
                    {
                        if (isLandData.Missions.TryGetValue(item.Id, out var vaule))
                        {
                            vaule.Progress += getCurrency;
                        }
                    }
                    break;
                default:
                    break;
            }

        }

    }

}