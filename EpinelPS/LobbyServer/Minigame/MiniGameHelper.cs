using EpinelPS.Database;
using EpinelPS.Data;
using Google.Protobuf.WellKnownTypes;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Minigame;


public class MiniGameHelper
{

    /// <summary>
    /// 获取工会排行榜
    /// </summary>
    /// <param name="guildId">工会id</param>
    /// <param name="arcadeId">游戏id</param>
    /// <returns></returns>
    public static IEnumerable<(ArcadeScoreRecord Record, int Rank)> GetFullLeaderboard(long guildId, int arcadeId)
    {
        RankData rank = JsonDb.GetRank();
        var result = new List<(ArcadeScoreRecord Record, int Rank)>();

        // 从字典中获取该 ArcadeId 的所有记录
        if (!rank.ArcadeScore.TryGetValue(arcadeId, out var records) || records.Count == 0)
            return result;

        // 筛选指定 GuildId 的记录，按 UserId 分组求和
        var grouped = records
            .Where(x => x.GuildId == guildId)
            .GroupBy(x => x.UserId)
            .Select(g => new
            {
                UserId = g.Key,
                TotalScore = g.Sum(x => x.Score),
                ModeId = g.First().ModeId  // 如果 ModeId 可能不同，需要根据业务决定
            })
            .OrderByDescending(x => x.TotalScore)
            .ToList();

        // 计算排名
        for (int i = 0; i < grouped.Count; i++)
        {
            var record = new ArcadeScoreRecord
            {
                UserId = grouped[i].UserId,
                Score = grouped[i].TotalScore,
                ModeId = grouped[i].ModeId,
                ArcadeId = arcadeId,
                GuildId = guildId
            };
            result.Add((record, i + 1));
        }

        return result;
    }

    public static NetMiniGameStellarBladeRankingData GetSBUserRank(ulong userid, int arcadeId)
    {
        IEnumerable<(ArcadeScoreRecord Record, int Rank)>? allBoard = GetArcadeBoard(arcadeId);

        if (allBoard.Count() > 0)
        {
            foreach (var item in allBoard)
            {
                if (item.Record.UserId == userid)
                {

                    NetMiniGameStellarBladeRankingData? rankdata = new NetMiniGameStellarBladeRankingData()
                    {
                        Rank = item.Rank,
                        Score = item.Record.Score,
                        User = LobbyHandler.CreateWholeUserDataFromDbUser(item.Record.UserId)
                    };

                    return rankdata;
                }

            }
        }

        return null;
    }


    /// <summary>
    /// 获取游戏排行榜
    /// </summary>    
    /// <param name="arcadeId">游戏id</param>
    /// <returns></returns>
    public static IEnumerable<(ArcadeScoreRecord Record, int Rank)> GetArcadeBoard(int arcadeId)
    {
        RankData rank = JsonDb.GetRank();
        var result = new List<(ArcadeScoreRecord Record, int Rank)>();

        // 从字典中获取该 ArcadeId 的所有记录
        if (!rank.ArcadeScore.TryGetValue(arcadeId, out var records) || records.Count == 0)
            return result;

        // 按 UserId 分组求和，并计算排名
        var grouped = records
            .GroupBy(x => x.UserId)
            .Select(g => new
            {
                UserId = g.Key,
                TotalScore = g.Sum(x => x.Score),
                ModeId = g.First().ModeId,  // 如果 ModeId 可能不同，需要根据业务决定
                GuildId = g.First().GuildId
            })
            .OrderByDescending(x => x.TotalScore)
            .ToList();

        // 计算排名
        for (int i = 0; i < grouped.Count; i++)
        {
            var record = new ArcadeScoreRecord
            {
                UserId = grouped[i].UserId,
                Score = grouped[i].TotalScore,
                ModeId = grouped[i].ModeId,
                ArcadeId = arcadeId,
                GuildId = grouped[i].GuildId
            };
            result.Add((record, i + 1));
        }

        return result;
    }


    /// <summary>
    /// 插入或更新
    /// </summary>
    public static void InsertOrUpdate(int arcadeId, ulong userId, int guildId, long score, int modeId)
    {
        RankData rank = JsonDb.GetRank();

        // 确保字典中有该 ArcadeId 的列表
        if (!rank.ArcadeScore.TryGetValue(arcadeId, out var list))
        {
            list = [];
            rank.ArcadeScore[arcadeId] = list;
        }

        // 查找是否已有相同记录（UserId + GuildId + ModeId + ArcadeId）
        var existing = list.FirstOrDefault(x =>
            x.UserId == userId &&
            x.GuildId == guildId &&
            x.ModeId == modeId);

        if (existing != null)
        {
            // 更新现有记录
            existing.Score = score;
        }
        else
        {
            // 插入新记录
            var newRecord = new ArcadeScoreRecord
            {
                Id = list.Count > 0 ? list.Max(x => x.Id) + 1 : 1,
                UserId = userId,
                GuildId = guildId,
                Score = score,
                ArcadeId = arcadeId,
                ModeId = modeId
            };
            list.Add(newRecord);
        }

        JsonDb.Save();
    }


    public static void InitStellarBladeData(User user, int arcadeId)
    {
        if (!user.StellarBladeDatas.TryGetValue(arcadeId, out var stellar))
        {
            stellar = new StellarBladeDatas();
            user.StellarBladeDatas.TryAdd(arcadeId, stellar);

            EventSBManagerRecord_Raw? sbm = new();

            if (arcadeId < 1000)
            {
                sbm = GameData.Instance.EventSBManagerTable.Values
                    .Where(m => m.MinigameType == MiniGameSystemType.Normal).FirstOrDefault();
            }
            else
            {
                sbm = GameData.Instance.EventSBManagerTable.Values
                    .Where(m => m.MinigameType == MiniGameSystemType.Arcade).FirstOrDefault();
            }
            //任务
            List<EventSBMissionRecord_Raw>? alist = GameData.Instance.EventSBMissionTable.Values
                .Where(x => x.GroupId == sbm.MissionGroupId && x.MissionCategory == SBMissionCategory.Achievement)
                .ToList();
            foreach (var item in alist)
            {
                NetStellarBladeMissionData missionData = new()
                {
                    IsReceived = false,
                    MissionId = item.Id,
                    Progress = 0
                };
                stellar.MissionData.Add(missionData);
            }

            List<EventSBMissionRecord_Raw>? dlist = GameData.Instance.EventSBMissionTable.Values
                .Where(x => x.GroupId == sbm.MissionGroupId && x.MissionCategory == SBMissionCategory.DailyMission)
                .ToList();
            if (dlist.Count > 0)
            {
                foreach (var item in dlist)
                {
                    NetStellarBladeMissionData missionData = new()
                    {
                        IsReceived = false,
                        MissionId = item.Id,
                        Progress = 0
                    };
                    stellar.DailyMissionData.Add(missionData);
                }
            }

            List<EventSBMissionRecord_Raw>? plist = GameData.Instance.EventSBMissionTable.Values
                .Where(x => x.GroupId == sbm.MissionGroupId && x.MissionCategory == SBMissionCategory.DailyPoint)
                .ToList();
            if (plist.Count > 0)
            {
                foreach (var item in plist)
                {
                    NetStellarBladeMissionData missionData = new()
                    {
                        IsReceived = false,
                        MissionId = item.Id,
                        Progress = 0
                    };
                    stellar.DailyPointMissionData.Add(missionData);
                }
            }

            //资源
            List<EventSBCurrencyRecord_Raw>? curlist = GameData.Instance.EventSBCurrencyTable.Values.ToList();
            foreach (var item in curlist)
            {
                stellar.Currency.Add(new() { Amount = 0, CurrencyType = (int)item.CurrencyType });
            }


            //人物        
            EventSBCharacterRecord_Raw? charrd = GameData.Instance.EventSBCharacterTable.Values
                .Where(c => c.Id == sbm.CharacterId).FirstOrDefault();

            stellar.CharacterData = new()
            {
                DefaultAttackSkillId = charrd.DefaultAttackSkillId
            };

            List<int>? learnskilllist = GameData.Instance.EventSBCharacterSkillTable.Values
                .Where(s => s.GroupId == charrd.SkillGroupId && s.IsDefaultLearn == true)
                .Select(s => s.Id)
                .ToList();

            stellar.CharacterData.LearnedSkillIdList.AddRange(learnskilllist);
            foreach (SBCharacterEnhanceType type in System.Enum.GetValues<SBCharacterEnhanceType>())
            {
                stellar.CharacterData.EnhanceDataList.Add(new NetStellarBladeCharacterData.Types.NetEnhanceData() { EnhanceLevel = 1, EnhanceType = (int)type });
            }


            var first = GameData.Instance.EventSBStageTable.Values
                .Where(s => s.GroupId == sbm.StageGroupId).FirstOrDefault();

            stellar.LastEnteredStageId = first.Id;

            JsonDb.Save();
        }
    }


    public static void UpMission(ref StellarBladeDatas stellar, SBMissionType missionType, int missionGroupId,
        int stage = 0, int damage = 0, int buyCount = 0, int usePotion = 0, int PerfectGuard = 0, int PerfectDodge = 0,
        int dailyPoint = 0, int playTime = 0)
    {
        switch (missionType)
        {
            case SBMissionType.StageClear:
                List<EventSBMissionRecord_Raw>? list = GameData.Instance.EventSBMissionTable.Values
                    .Where(x => x.GroupId == missionGroupId && x.MissionType == missionType && x.MissionTargetId == 0).ToList();
                foreach (var item in list)
                {
                    switch (item.MissionCategory)
                    {
                        case SBMissionCategory.DailyMission:
                            NetStellarBladeMissionData? dmiss = stellar.DailyMissionData.FirstOrDefault(m => m.MissionId == item.Id);
                            dmiss.Progress += 1;
                            break;
                        case SBMissionCategory.DailyPoint:
                            break;
                        case SBMissionCategory.Achievement:
                            NetStellarBladeMissionData? miss = stellar.MissionData.FirstOrDefault(m => m.MissionId == item.Id);
                            miss.Progress += 1;
                            break;
                        default:
                            break;
                    }


                }
                EventSBMissionRecord_Raw? spstage = GameData.Instance.EventSBMissionTable.Values
                    .Where(x => x.GroupId == missionGroupId && x.MissionType == missionType && x.MissionTargetId == stage).FirstOrDefault();
                if (spstage != null)
                {
                    NetStellarBladeMissionData? miss = stellar.MissionData.FirstOrDefault(m => m.MissionId == spstage.Id);
                    miss.Progress += 1;
                }
                break;
            case SBMissionType.PerfectGuard:
                List<EventSBMissionRecord_Raw>? glist = GameData.Instance.EventSBMissionTable.Values
                  .Where(x => x.GroupId == missionGroupId && x.MissionType == missionType).ToList();
                foreach (var item in glist)
                {
                    switch (item.MissionCategory)
                    {
                        case SBMissionCategory.DailyMission:
                            NetStellarBladeMissionData? dmiss = stellar.DailyMissionData.FirstOrDefault(m => m.MissionId == item.Id);
                            dmiss.Progress += PerfectGuard;
                            break;
                        case SBMissionCategory.DailyPoint:
                            break;
                        case SBMissionCategory.Achievement:
                            NetStellarBladeMissionData? miss = stellar.MissionData.FirstOrDefault(m => m.MissionId == item.Id);
                            miss.Progress += PerfectGuard;
                            break;
                        default:
                            break;
                    }
                }
                break;
            case SBMissionType.PerfectDodge:
                List<EventSBMissionRecord_Raw>? dolist = GameData.Instance.EventSBMissionTable.Values
                  .Where(x => x.GroupId == missionGroupId && x.MissionType == missionType).ToList();
                foreach (var item in dolist)
                {
                    switch (item.MissionCategory)
                    {
                        case SBMissionCategory.DailyMission:
                            NetStellarBladeMissionData? dmiss = stellar.DailyMissionData.FirstOrDefault(m => m.MissionId == item.Id);
                            dmiss.Progress += PerfectDodge;
                            break;
                        case SBMissionCategory.DailyPoint:
                            break;
                        case SBMissionCategory.Achievement:
                            NetStellarBladeMissionData? miss = stellar.MissionData.FirstOrDefault(m => m.MissionId == item.Id);
                            miss.Progress += PerfectDodge;
                            break;
                        default:
                            break;
                    }

                }
                break;
            case SBMissionType.Damage:
                List<EventSBMissionRecord_Raw>? dlist = GameData.Instance.EventSBMissionTable.Values
                    .Where(x => x.GroupId == missionGroupId && x.MissionType == missionType).ToList();
                foreach (var item in dlist)
                {
                    switch (item.MissionCategory)
                    {
                        case SBMissionCategory.DailyMission:
                            NetStellarBladeMissionData? dmiss = stellar.DailyMissionData.FirstOrDefault(m => m.MissionId == item.Id);
                            dmiss.Progress += damage;
                            break;
                        case SBMissionCategory.DailyPoint:
                            break;
                        case SBMissionCategory.Achievement:
                            NetStellarBladeMissionData? miss = stellar.MissionData.FirstOrDefault(m => m.MissionId == item.Id);
                            miss.Progress += damage;
                            break;
                        default:
                            break;
                    }
                }
                break;
            case SBMissionType.GainGoldCount:
                break;
            case SBMissionType.BuyItem:
                List<EventSBMissionRecord_Raw>? blist = GameData.Instance.EventSBMissionTable.Values
                    .Where(x => x.GroupId == missionGroupId && x.MissionType == missionType).ToList();
                foreach (var item in blist)
                {
                    NetStellarBladeMissionData? miss = stellar.MissionData.FirstOrDefault(m => m.MissionId == item.Id);
                    miss.Progress += buyCount;
                }
                break;
            case SBMissionType.PlayTime:
                List<EventSBMissionRecord_Raw>? tlist = GameData.Instance.EventSBMissionTable.Values
                  .Where(x => x.GroupId == missionGroupId && x.MissionType == missionType).ToList();
                foreach (var item in tlist)
                {
                    switch (item.MissionCategory)
                    {
                        case SBMissionCategory.DailyMission:
                            NetStellarBladeMissionData? dmiss = stellar.DailyMissionData.FirstOrDefault(m => m.MissionId == item.Id);
                            dmiss.Progress += playTime;
                            break;
                        case SBMissionCategory.DailyPoint:
                            break;
                        case SBMissionCategory.Achievement:
                            NetStellarBladeMissionData? miss = stellar.MissionData.FirstOrDefault(m => m.MissionId == item.Id);
                            miss.Progress += playTime;
                            break;
                        default:
                            break;
                    }

                }
                break;
            case SBMissionType.UsePotion:
                List<EventSBMissionRecord_Raw>? plist = GameData.Instance.EventSBMissionTable.Values
                   .Where(x => x.GroupId == missionGroupId && x.MissionType == missionType).ToList();
                foreach (var item in plist)
                {
                    switch (item.MissionCategory)
                    {
                        case SBMissionCategory.DailyMission:
                            NetStellarBladeMissionData? dmiss = stellar.DailyMissionData.FirstOrDefault(m => m.MissionId == item.Id);
                            dmiss.Progress += usePotion;
                            break;
                        case SBMissionCategory.DailyPoint:
                            break;
                        case SBMissionCategory.Achievement:
                            NetStellarBladeMissionData? miss = stellar.MissionData.FirstOrDefault(m => m.MissionId == item.Id);
                            miss.Progress += usePotion;
                            break;
                        default:
                            break;
                    }

                }
                break;
            case SBMissionType.DailyPointReward:
                List<EventSBMissionRecord_Raw>? dailist = GameData.Instance.EventSBMissionTable.Values
                    .Where(x => x.GroupId == missionGroupId && x.MissionType == missionType).ToList();
                foreach (var item in dailist)
                {
                    NetStellarBladeMissionData? miss = stellar.DailyPointMissionData.FirstOrDefault(m => m.MissionId == item.Id);
                    miss.Progress += dailyPoint;
                }
                break;
            default:
                break;
        }

        JsonDb.Save();
    }


    public static void GetReward(ref NetStellarBladeRewardData ret, ref StellarBladeDatas stellar, int rewardGroupId, int score = 0)
    {
        List<EventSBStageRewardRecord_Raw>? rewards = GameData.Instance.EventSBStageRewardTable.Values
            .Where(f => f.GroupId == rewardGroupId)
            .ToList();

        if (rewards.Count == 0) return;

        // 根据分数获取对应的奖励（单条或多条时按分数筛选）
        EventSBStageRewardRecord_Raw? reward = rewards.Count == 1
            ? rewards[0]
            : rewards.FirstOrDefault(r => score >= r.ScoreMin && (r.ScoreMax == 0 || score <= r.ScoreMax));

        if (reward == null) return;

        // 处理奖励
        switch (reward.RewardType)
        {
            case SBRewardType.SBCurrency:

                EventSBCurrencyRecord_Raw? ctype = GameData.Instance.EventSBCurrencyTable.Values
                    .Where(x => x.Id == reward.RewardId).FirstOrDefault();


                NetStellarBladeCurrency? existing = ret.CurrencyList.FirstOrDefault(c => c.CurrencyType == (int)ctype.CurrencyType);
                if (existing != null)
                {
                    existing.Amount += reward.RewardAmount;
                }
                else
                {
                    ret.CurrencyList.Add(new NetStellarBladeCurrency
                    {
                        CurrencyType = (int)ctype.CurrencyType,
                        Amount = reward.RewardAmount
                    });
                }

                NetStellarBladeCurrency? currency = stellar.Currency.FirstOrDefault(c => c.CurrencyType == (int)ctype.CurrencyType);
                if (currency != null)
                {
                    currency.Amount += reward.RewardAmount;
                }

                JsonDb.Save();
                break;
            case SBRewardType.SBItem:
                // TODO: 处理道具奖励
                break;
        }
    }


    public static List<int> GetMissionReward(ref NetStellarBladeRewardData ret, ref StellarBladeDatas stellar, User user, EventSBMissionRecord_Raw mission)
    {
        List<int> result = new List<int>();

        int dateDay = user.GetDateDay();
        switch (mission.RewardType)
        {
            case SBMissionRewardType.DailyPoint:
                List<EventSBMissionRecord_Raw>? glist = GameData.Instance.EventSBMissionTable.Values
                    .Where(x => x.GroupId == mission.GroupId && x.MissionType == SBMissionType.DailyPointReward).ToList();
                foreach (var item in glist)
                {
                    NetStellarBladeMissionData? miss = stellar.MissionData.FirstOrDefault(m => m.MissionId == item.Id);
                    if (stellar.Today == dateDay)
                    {
                        miss.Progress += mission.RewardValue;
                    }
                    else
                    {
                        miss.Progress = mission.RewardValue;
                    }
                }
                break;
            case SBMissionRewardType.RewardId:
                result.Add(mission.RewardValue);
                break;
            case SBMissionRewardType.SBItem:
                break;
            case SBMissionRewardType.SBCurrency:
                break;
            default:
                break;
        }

        JsonDb.Save();
        return result;
    }

    public static TowerDefenseData InitTDData(int aid, User user)
    {
        TowerDefenseData defenseData = new TowerDefenseData
        {
            ChallengeMaxScore = 0,
            ClearedStageIdList = [],
            ClearedTutorialIdList = [],
            UpgradeCurrency = 0,
            UpgradeIdList = []
        };

        ArcadeManagerRecord_Raw? arcadeid = GameData.Instance.ArcadeManagerTable.Values
            .Where(x => x.Id == aid).FirstOrDefault();
        if (arcadeid != null)
        {
            EventTowerDefenseArcadeManagerRecord_Raw? tddata = GameData.Instance.EventTowerDefenseArcadeManagerTable.Values
                .Where(x => x.Id == arcadeid.GameManagerId).FirstOrDefault();
            if (tddata != null)
            {
                List<EventTowerDefenseMissionRecord>? misslist = GameData.Instance.EventTowerDefenseMissionTable.Values
                    .Where(x => x.ArcadeMissionGroupId == tddata.ArcadeMissionGroupId).ToList();

                foreach (var item in misslist)
                {
                    defenseData.MissionProgressList.Add(new NetArcadeTowerDefenseMissionProgress()
                    {
                        CreatedAt = DateTime.UtcNow.Date.ToTimestamp(),
                        MissionTid = item.Id,
                        MissionUid = item.Id,
                        Progress = 0
                    });

                }
            }
        }

        user.TowerDefenseDatas.TryAdd(aid, defenseData);
        JsonDb.Save();

        return defenseData;
    }



}
