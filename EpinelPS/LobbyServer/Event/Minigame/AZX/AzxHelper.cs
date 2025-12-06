using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Utils;
using Google.Protobuf.Collections;
using Google.Protobuf.WellKnownTypes;
using log4net;
using Newtonsoft.Json;

namespace EpinelPS.LobbyServer.Event.Minigame.AZX
{
    public static class AzxHelper
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(AzxHelper));

        public static void AcquireReward(User user, ref NetRewardData reward, int azxId, RepeatedField<int> missionIds)
        {
            log.Debug($"Acquiring reward for user {user.ID}, azxId: {azxId}, missionIds: {JsonConvert.SerializeObject(missionIds)}");
            if (missionIds.Count == 0 || azxId == 0) return;
            try
            {
                var missions = GameData.Instance.EventAZXAppleGameMissionTable.Values.Where(x =>
                    x.MissionRewardId > 0 && missionIds.Contains(x.Id)).ToList();
                if (missions.Count == 0) return;
                List<Reward_Data> rewardDatas = [];
                foreach (var mission in missions)
                {
                    var rewardRecord = GameData.Instance.GetRewardTableEntry(mission.MissionRewardId);
                    if (rewardRecord is null || rewardRecord.Rewards.Count == 0) continue;
                    foreach (var rewardItem in rewardRecord.Rewards)
                    {
                        if (rewardItem.RewardValue == 0) continue;
                        int itemIndex = rewardDatas.FindIndex(x => x.RewardId == rewardItem.RewardId);
                        if (itemIndex >= 0)
                            rewardDatas[itemIndex].RewardValue += rewardItem.RewardValue;
                        else
                            rewardDatas.Add(rewardItem);
                    }
                }
                foreach (var rewardData in rewardDatas)
                {
                    RewardUtils.AddSingleObject(user, ref reward, rewardData.RewardId, rewardData.RewardType, rewardData.RewardValue);
                }

                var azxInfo = GetAzxInfo(user, azxId);
                azxInfo.AchievementMissionDataList.AddRange(missionIds.Select(x =>
                    new AchievementMissionData() { MissionId = x, IsReceived = true }));

                user.MiniGameAzxInfo[azxId] = azxInfo;
                JsonDb.Save();
            }
            catch (Exception ex)
            {
                Logging.WriteLine($"Acquiring reward failed: {ex.Message}", LogType.Error);
            }
        }

        public static void GetRanking(User user, int azxId, ref ResGetMiniGameAzxRanking response)
        {
            // ResGetMiniGameAzxRanking Fields
            //  NetMiniGameAzxRankingData UserGuildRanking
            //  RepeatedField<NetMiniGameAzxRankingData> GuildRankingList
            // NetMiniGameAzxRankingData Fields
            //  int Rank
            //  NetMiniGameAzxScoreAndTime ScoreAndTime
            //  NetWholeUserData User
            //  NetMiniGameAzxScoreAndTime Fields
            //      int Score
            //      Google.Protobuf.WellKnownTypes.Duration TimeToScore

            int dateDay = GetDateDay();
            var azxInfo = GetAzxInfo(user, azxId);
            var scoreData = azxInfo.ScoreDatas.Find(x => x.DateDay == dateDay && x.AzxId == azxId);
            int score = scoreData?.HighScore ?? 0;
            Duration timeToScore = scoreData?.HighScoreTime ?? new Duration() { Seconds = 0, Nanos = 0 };

            response.UserGuildRanking = new NetMiniGameAzxRankingData()
            {
                Rank = 1,
                ScoreAndTime = new NetMiniGameAzxScoreAndTime()
                {
                    Score = score,
                    TimeToScore = timeToScore
                },
                User = new NetWholeUserData()
                {
                    Usn = (long)user.ID,
                    Server = 10001,
                    Nickname = user.Nickname,
                    Lv = user.userPointData?.UserLevel ?? 99,
                    Icon = user.ProfileIconId,
                    IconPrism = user.ProfileIconIsPrism,
                    Frame = user.ProfileFrame,
                    LastActionAt = user.LastLogin.Ticks,
                    UserTitleId = user.TitleId,
                    GuildName = user.Nickname,
                }
            };
            response.GuildRankingList.Add(new NetMiniGameAzxRankingData()
            {
                Rank = 2,
                ScoreAndTime = new NetMiniGameAzxScoreAndTime()
                {
                    Score = 80000,
                    TimeToScore = new Duration() { Seconds = 118, Nanos = 432877000 }
                },
                User = new NetWholeUserData()
                {
                    Usn = (long)user.ID,
                    Server = 10001,
                    Nickname = user.Nickname,
                    Lv = user.userPointData?.UserLevel ?? 99,
                    Icon = user.ProfileIconId,
                    IconPrism = user.ProfileIconIsPrism,
                    Frame = user.ProfileFrame,
                    LastActionAt = user.LastLogin.Ticks,
                    UserTitleId = user.TitleId,
                    GuildName = user.Nickname,
                }
            });

            JsonDb.Save();
        }

        public static void FinishAzx(User user, ReqFinishMiniGameAzx req, ref ResFinishMiniGameAzx response)
        {
            // ReqEnterMiniGameAzx Fields
            //  int AzxId
            //  NetMiniGameAzxScoreAndTime ScoreAndTime
            //  int PlayBoardId
            //  int PlayCharacterId
            //  RepeatedField<NetSkillUseCountData> SkillUseCountList
            //  int CutSceneId

            // ResFinishMiniGameAzx Fields
            //  NetRewardData Reward
            //  NetMiniGameAzxDailyMissionData DailyMissionData
            //  bool IsNewHighScore
            //  bool IsNewHighScoreTime
            try
            {
                log.Debug($"Finishing AZX for user {user.ID}, data: {JsonConvert.SerializeObject(req)}");
                int dateDay = GetDateDay();

                int score = req.ScoreAndTime.Score;
                Duration timeToScore = req.ScoreAndTime.TimeToScore;

                NetMiniGameAzxDailyMissionData dailyMissionData = new() { IsDailyRewarded = false, DailyAccumulatedScore = 0 };
                NetRewardData reward = new();

                var azxInfo = GetAzxInfo(user, req.AzxId);

                if (req.CutSceneId > 0 && azxInfo.CutSceneDataList.Find(x => x.CutSceneId == req.CutSceneId) is null)
                {
                    azxInfo.CutSceneDataList.Add(new CutSceneData() { CutSceneId = req.CutSceneId, IsNew = true });
                }

                var scoreDataIndex = azxInfo.ScoreDatas.FindIndex(x => x.DateDay == dateDay && x.AzxId == req.AzxId);
                if (scoreDataIndex >= 0)
                {
                    if (score > 10000 && !azxInfo.ScoreDatas[scoreDataIndex].IsDailyRewarded)
                    {
                        azxInfo.ScoreDatas[scoreDataIndex].IsDailyRewarded = true;
                        dailyMissionData.IsDailyRewarded = true;
                        RewardUtils.AddSingleCurrencyObject(user, ref reward, CurrencyType.FreeCash, 30);
                    }
                    azxInfo.ScoreDatas[scoreDataIndex].AccumulatedScore += score;
                    dailyMissionData.DailyAccumulatedScore = azxInfo.ScoreDatas[scoreDataIndex].AccumulatedScore;
                    if (azxInfo.ScoreDatas[scoreDataIndex].HighScore < score)
                    {
                        response.IsNewHighScore = true;
                        azxInfo.ScoreDatas[scoreDataIndex].HighScore = score;
                    }
                    if (azxInfo.ScoreDatas[scoreDataIndex].HighScoreTime.ToTimeSpan() > timeToScore.ToTimeSpan())
                    {
                        response.IsNewHighScoreTime = true;
                        azxInfo.ScoreDatas[scoreDataIndex].HighScoreTime = timeToScore;
                    }
                }
                else
                {
                    bool isDailyRewarded = false;
                    if (score > 10000)
                    {
                        isDailyRewarded = true;
                        dailyMissionData.IsDailyRewarded = true;
                        RewardUtils.AddSingleCurrencyObject(user, ref reward, CurrencyType.FreeCash, 30);
                    }
                    response.IsNewHighScoreTime = true;
                    response.IsNewHighScore = true;
                    azxInfo.ScoreDatas.Add(new MiniGameAzxScoreData
                    {
                        AzxId = req.AzxId,
                        DateDay = dateDay,
                        AccumulatedScore = score,
                        HighScore = score,
                        HighScoreTime = timeToScore,
                        IsDailyRewarded = isDailyRewarded
                    });
                    dailyMissionData.DailyAccumulatedScore = score;
                }

                //  int PlayBoardId
                //  int PlayCharacterId
                azxInfo.CharacterCount ??= [];
                if (azxInfo.CharacterCount.TryGetValue(req.PlayCharacterId, out var characterCount))
                    azxInfo.CharacterCount[req.PlayCharacterId] = characterCount + 1;
                else
                    azxInfo.CharacterCount.Add(req.PlayCharacterId, 1);
                //  RepeatedField<NetSkillUseCountData> SkillUseCountList
                if (req.SkillUseCountList != null && req.SkillUseCountList.Count > 0)
                {
                    azxInfo.SkillCount ??= [];
                    foreach (var item in req.SkillUseCountList)
                    {
                        if (azxInfo.SkillCount.TryGetValue(item.SkillId, out var skillCount))
                            azxInfo.SkillCount[item.SkillId] = skillCount + item.SkillUseCount;
                        else
                            azxInfo.SkillCount.Add(item.SkillId, item.SkillUseCount);
                    }
                }

                response.DailyMissionData = dailyMissionData;
                response.Reward = reward;
                // Save changes
                user.MiniGameAzxInfo[req.AzxId] = azxInfo;
                JsonDb.Save();
            }
            catch (Exception ex)
            {
                Logging.WriteLine($"Finish AZX Error: {ex.Message}", LogType.Error);
            }
        }

        public static void EnterAzx(User user, ref ResEnterMiniGameAzx response, int azxId, int playBoardId, int playCharacterId)
        {
            log.Debug($"Entering AZX AzxId: {azxId}, PlayBoardId: {playBoardId}, PlayCharacterId: {playCharacterId}");
            try
            {
                var azxInfo = GetAzxInfo(user, azxId);
                azxInfo.SelectedBoardId = playBoardId;
                azxInfo.SelectedCharacterId = playCharacterId;
                user.MiniGameAzxInfo[azxId] = azxInfo;
                response.PreviousSRankCount = 0;
            }
            catch (Exception ex)
            {
                Logging.WriteLine($"Enter AZX Error: {ex.Message}", LogType.Error);
            }
        }

        public static void GetAzxData(User user, int azxId, ref ResGetMiniGameAzxData response)
        {
            log.Debug($"Getting AZX data for user {user.ID}");

            var azxInfo = GetAzxInfo(user, azxId);
            // Initialize score data if null
            azxInfo.ScoreDatas ??= [];
            // Get sum score
            int sumScore = azxInfo.ScoreDatas.Sum(x => x.AccumulatedScore);

            log.Debug($"AZX data: {JsonConvert.SerializeObject(azxInfo)}");
            try
            {
                // AchievementMissionDataList
                var missions = GameData.Instance.EventAZXAppleGameMissionTable.Values.ToList();

                foreach (var mission in missions)
                {
                    bool isReceived = false;
                    azxInfo.AchievementMissionDataList ??= [];
                    var item = azxInfo.AchievementMissionDataList.Find(x => x.MissionId == mission.Id);
                    if (item is not null) isReceived = item.IsReceived;
                    if (mission.MissionType == EventAZXAppleGameMissionMissionType.GetScore)
                    {
                        AddAchievementMission(ref response, mission.Id, sumScore, isReceived);
                    }
                    else if (mission.MissionType == EventAZXAppleGameMissionMissionType.UseSkillCount)
                    {
                        int progress = 0;
                        if (azxInfo.SkillCount.TryGetValue(mission.MissionConditionId, out var skillCount)) progress = skillCount;
                        AddAchievementMission(ref response, mission.Id, progress, isReceived);
                    }
                    else if (mission.MissionType == EventAZXAppleGameMissionMissionType.PlayCharacterCount)
                    {
                        int progress = 0;
                        if (azxInfo.CharacterCount.TryGetValue(mission.MissionConditionId, out var characterCount)) progress = characterCount;
                        AddAchievementMission(ref response, mission.Id, progress, isReceived);
                    }
                    else
                    {
                        AddAchievementMission(ref response, mission.Id, 0, isReceived);
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error($"Get AchievementMissionDataList Error: {ex.Message}");
            }

            try
            {
                // ConditionalBoardDataList
                azxInfo.ConditionalBoardDataList ??= [];
                var boards = GameData.Instance.EventAZXAppleGameBoardTable.Values.ToList();
                foreach (var board in boards)
                {
                    var item = azxInfo.ConditionalBoardDataList.Find(x => x.BoardId == board.Id);
                    bool isUnlocked = item is not null && item.IsUnlocked;
                    if (board.BoardOpenScore == 0) continue;
                    response.ConditionalBoardDataList.Add(new NetMiniGameAzxConditionalBoardData()
                    {
                        BoardId = board.Id,
                        Progress = sumScore,
                        IsUnlocked = isUnlocked
                    });
                }
                // SelectedBoardId
                int selectedBoardId = azxInfo.SelectedBoardId > 0 ? azxInfo.SelectedBoardId : boards.Min(x => x.Id);
                response.SelectedBoardId = selectedBoardId;
            }
            catch (Exception ex)
            {
                log.Error($"Get ConditionalBoardDataList Error: {ex.Message}");
            }

            try
            {
                // ConditionalCharacterDataList
                azxInfo.ConditionalCharacterDataList ??= [];
                var characters = GameData.Instance.EventAZXAppleGameCharacterTable.Values.ToList();
                foreach (var character in characters)
                {
                    var item = azxInfo.ConditionalCharacterDataList.Find(x => x.CharacterId == character.Id);
                    bool isUnlocked = item is not null && item.IsUnlocked;
                    if (character.CharacterOpenScore == 0) continue;
                    response.ConditionalCharacterDataList.Add(new NetMiniGameAzxConditionalCharacterData()
                    {
                        CharacterId = character.Id,
                        Progress = sumScore,
                        IsUnlocked = isUnlocked
                    });
                }
                // SelectedCharacterId
                int selectedCharacterId = azxInfo.SelectedCharacterId > 0 ? azxInfo.SelectedCharacterId : characters.Min(x => x.Id);
                response.SelectedCharacterId = selectedCharacterId;
            }
            catch (Exception ex)
            {
                log.Error($"Get ConditionalCharacterDataList Error: {ex.Message}");
            }

            try
            {
                // ConditionalSkillDataList
                azxInfo.ConditionalSkillDataList ??= [];
                foreach (var skill in GameData.Instance.EventAZXAppleGameSkillTable.Values)
                {
                    var item = azxInfo.ConditionalSkillDataList.Find(x => x.SkillId == skill.Id);
                    bool isUnlocked = item is not null && item.IsUnlocked;
                    if (skill.SkillOpenUseValue == 0) continue;
                    response.ConditionalSkillDataList.Add(new NetMiniGameAzxConditionalSkillData()
                    {
                        SkillId = skill.Id,
                        IsUnlocked = isUnlocked
                    });
                }
            }
            catch (Exception ex)
            {
                log.Error($"Get ConditionalSkillDataList Error: {ex.Message}");
            }

            try
            {
                azxInfo.CutSceneDataList ??= [];
                foreach (var cutScene in GameData.Instance.EventAZXAppleGameCutSceneTable.Values)
                {
                    var item = azxInfo.CutSceneDataList.Find(x => x.CutSceneId == cutScene.Id);
                    if (item is not null && item.CutSceneId > 0)
                    {
                        response.CutSceneList.Add(new NetMiniGameAzxCutSceneData { CutSceneId = cutScene.Id, IsNew = item.IsNew });
                    }
                }
            }
            catch (Exception ex)
            {
                Logging.WriteLine($"Get CutSceneList Error: {ex.Message}", LogType.Error);
            }

            try
            {
                // NetMiniGameAzxDailyMissionData DailyMissionData
                azxInfo.ScoreDatas ??= [];
                int dateDay = GetDateDay();
                var scoreData = azxInfo.ScoreDatas.Find(x => x.DateDay == dateDay && x.AzxId == azxId);
                if (scoreData is not null)
                {
                    response.DailyMissionData = new NetMiniGameAzxDailyMissionData()
                    {
                        DailyAccumulatedScore = scoreData.AccumulatedScore,
                        IsDailyRewarded = scoreData.IsDailyRewarded,
                    };
                }
            }
            catch (Exception ex)
            {
                Logging.WriteLine($"Get DailyMissionData Error: {ex.Message}", LogType.Error);
            }

            response.IsTutorialConfirmed = azxInfo.IsTutorialConfirmed;
        }

        public static void SetTutorialConfirmed(User user, int azxId)
        {
            log.Debug($"Setting tutorial confirmed for AZX {azxId}");
            try
            {
                var azxInfo = GetAzxInfo(user, azxId);
                azxInfo.IsTutorialConfirmed = true;
                user.MiniGameAzxInfo[azxId] = azxInfo;
                log.Debug($"Tutorial data after: {azxInfo.IsTutorialConfirmed}");
            }
            catch (Exception ex)
            {
                Logging.WriteLine($"Setting AZX tutorial confirmed failed: {ex.Message}", LogType.Error);
            }
        }

        public static void SetBoardUnlocked(User user, int azxId, int boardId)
        {
            log.Debug($"Setting board {boardId} unlocked for AZX {azxId}");
            try
            {

                var azxInfo = GetAzxInfo(user, azxId);
                azxInfo.ConditionalBoardDataList ??= [];
                log.Debug($"Board data before: {JsonConvert.SerializeObject(azxInfo.ConditionalBoardDataList)}");
                var itemIndex = azxInfo.ConditionalBoardDataList.FindIndex(x => x.BoardId == boardId);
                if (itemIndex >= 0)
                {
                    azxInfo.ConditionalBoardDataList[itemIndex].IsUnlocked = true;
                }
                else
                {
                    azxInfo.ConditionalBoardDataList.Add(new ConditionalBoardData() { BoardId = boardId, IsUnlocked = true });
                }
                user.MiniGameAzxInfo[azxId] = azxInfo;
                log.Debug($"Board data after: {JsonConvert.SerializeObject(azxInfo.ConditionalBoardDataList)}");
            }
            catch (Exception ex)
            {
                Logging.WriteLine($"Setting AZX board unlocked failed: {ex.Message}", LogType.Error);
            }
        }

        public static void SetSkillUnlocked(User user, int azxId, int skillId)
        {
            log.Debug($"Setting skill {skillId} unlocked for AZX {azxId}");
            try
            {
                var azxInfo = GetAzxInfo(user, azxId);
                azxInfo.ConditionalSkillDataList ??= [];
                log.Debug($"Skill data before: {JsonConvert.SerializeObject(azxInfo.ConditionalSkillDataList)}");
                var itemIndex = azxInfo.ConditionalSkillDataList.FindIndex(x => x.SkillId == skillId);
                if (itemIndex >= 0)
                {
                    azxInfo.ConditionalSkillDataList[itemIndex].IsUnlocked = true;
                }
                else
                {
                    azxInfo.ConditionalSkillDataList.Add(new ConditionalSkillData() { SkillId = skillId, IsUnlocked = true });
                }
                user.MiniGameAzxInfo[azxId] = azxInfo;
                log.Debug($"Skill data after: {JsonConvert.SerializeObject(azxInfo.ConditionalSkillDataList)}");
            }
            catch (Exception ex)
            {
                Logging.WriteLine($"Setting AZX skill unlocked failed: {ex.Message}", LogType.Error);
            }
        }

        public static void SetCharacterUnlocked(User user, int azxId, int characterId)
        {
            log.Debug($"Setting character {characterId} unlocked for AZX {azxId}");
            try
            {
                var azxInfo = GetAzxInfo(user, azxId);
                azxInfo.ConditionalCharacterDataList ??= [];
                log.Debug($"Character data before: {JsonConvert.SerializeObject(azxInfo.ConditionalCharacterDataList)}");
                var itemIndex = azxInfo.ConditionalCharacterDataList.FindIndex(x => x.CharacterId == characterId);
                if (itemIndex >= 0)
                {
                    azxInfo.ConditionalCharacterDataList[itemIndex].IsUnlocked = true;
                }
                else
                {
                    azxInfo.ConditionalCharacterDataList.Add(new ConditionalCharacterData() { CharacterId = characterId, IsUnlocked = true });
                }
                user.MiniGameAzxInfo[azxId] = azxInfo;
                log.Debug($"Character data after: {JsonConvert.SerializeObject(azxInfo.ConditionalCharacterDataList)}");
            }
            catch (Exception ex)
            {
                Logging.WriteLine($"Setting AZX character unlocked failed: {ex.Message}", LogType.Error);
            }
        }

        public static void SetCutSceneConfirmed(User user, int azxId, List<int> cutSceneIdList)
        {
            log.Debug($"Setting cutscenes confirmed for AZX {azxId}: {string.Join(", ", cutSceneIdList)}");
            try
            {
                var azxInfo = GetAzxInfo(user, azxId);
                azxInfo.CutSceneDataList ??= [];
                log.Debug($"Cutscene data before: {JsonConvert.SerializeObject(azxInfo.CutSceneDataList)}");
                foreach (var item in cutSceneIdList)
                {
                    var itemIndex = azxInfo.CutSceneDataList.FindIndex(x => x.CutSceneId == item);
                    if (itemIndex >= 0)
                    {
                        azxInfo.CutSceneDataList[itemIndex].IsNew = false;
                    }
                    else
                    {
                        azxInfo.CutSceneDataList.Add(new CutSceneData() { CutSceneId = item, IsNew = false });
                    }
                }
                user.MiniGameAzxInfo[azxId] = azxInfo;
                log.Debug($"Cutscene data after: {JsonConvert.SerializeObject(azxInfo.CutSceneDataList)}");
            }
            catch (Exception ex)
            {
                Logging.WriteLine($"Setting AZX cutscene confirmed failed: {ex.Message}", LogType.Error);
            }
        }

        private static int GetDateDay()
        {
            // +4 每天4点重新计算
            DateTime dateTime = DateTime.UtcNow.AddHours(4);
            return dateTime.Year * 10000 + dateTime.Month * 100 + dateTime.Day;
        }

        private static void AddAchievementMission(ref ResGetMiniGameAzxData response, int missionId, int progress, bool isReceived)
        {
            response.AchievementMissionDataList.Add(new NetMiniGameAzxAchievementMissionData()
            {
                MissionId = missionId,
                Progress = progress,
                IsReceived = isReceived
            });
        }

        public static MiniGameAzxData GetAzxInfo(User user, int azxId)
        {
            if (!user.MiniGameAzxInfo.TryGetValue(azxId, out var azxInfo))
            {
                log.Debug($"Creating new AZX info for {azxId}");
                user.MiniGameAzxInfo.TryAdd(azxId, new MiniGameAzxData() { });
                azxInfo = new MiniGameAzxData() { };
            }
            log.Debug($"Getting AZX info for {azxId}, data: {JsonConvert.SerializeObject(azxInfo)}");
            return azxInfo;
        }


    }
}