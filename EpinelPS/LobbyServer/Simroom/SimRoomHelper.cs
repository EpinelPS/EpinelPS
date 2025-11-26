using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Utils;
using log4net;
using Newtonsoft.Json;

namespace EpinelPS.LobbyServer.Simroom
{
    public static class SimRoomHelper
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(SimRoomHelper));
        private static readonly Random _rand = new();

        /// <summary>
        /// Get SimRoomEvent By User or DifficultyId and ChapterId
        /// </summary>
        /// <param name="user"> User </param>
        /// <param name="difficultyId"> DifficultyId </param>
        /// <param name="chapterId"> ChapterId </param>
        /// <returns>The list of NetSimRoomEvent</returns>
        public static List<NetSimRoomEvent> GetSimRoomEvents(User user, int difficultyId = 0, int chapterId = 0)
        {
            List<NetSimRoomEvent> netSimRooms = [];
            int currentDifficulty = user.ResetableData.SimRoomData.CurrentDifficulty;
            int currentChapter = user.ResetableData.SimRoomData.CurrentChapter;

            if (difficultyId > 0) currentDifficulty = difficultyId;
            if (chapterId > 0) currentChapter = chapterId;

            var events = user.ResetableData.SimRoomData.Events;
            if (events.Count > 1)
            {
                return [.. events.Select(MToNet)];
            }

            var simRoomChapter = GameData.Instance.SimulationRoomChapterTable.Values.FirstOrDefault(x => x.Chapter == currentChapter && x.DifficultyId == currentDifficulty);
            log.Debug($"Fond SimulationRoomChapter Chapter: {currentChapter}, DifficultyId: {currentDifficulty} Data: {JsonConvert.SerializeObject(simRoomChapter)}");

            var buffPreviewRecords = GameData.Instance.SimulationRoomBuffPreviewTable.Values.ToList();
            var simRoomBattleEventRecords = GameData.Instance.SimulationRoomBattleEventTable.Values.ToList();
            var simRoomSelectionEventRecords = GameData.Instance.SimulationRoomSelectionEventTable.Values.ToList();
            var simRoomSelectionEventGroupRecords = GameData.Instance.SimulationRoomSelectionGroupTable.Values.ToList();

            if (simRoomChapter is null) return netSimRooms;

            for (int i = 1; i <= simRoomChapter.StageValue; i++)
            {
                if (i == simRoomChapter.StageValue) // BossBattle
                {
                    var battleBuffPreviews = GameData.Instance.SimulationRoomBuffPreviewTable.Values.Where(x => x.EventType is SimulationRoomEvent.BossBattle).ToList();
                    var randomBuffPreview = GetRandomItems(battleBuffPreviews, 1);
                    var simRoomBattleEvent = CreateSimRoomBattleEvent(chapter: simRoomChapter, stage: i, order: 1, simRoomBattleEventRecords, randomBuffPreview[0]);
                    events.Add(NetToM(simRoomBattleEvent));
                    netSimRooms.Add(simRoomBattleEvent);
                }
                else if (i == simRoomChapter.StageValue - 1) // Selection
                {
                    // Maintenance Selection
                    var simRoomSelectionEventRecord = simRoomSelectionEventRecords.OrderBy(x => x.Id).ToList()
                        .FindLast(x => x.EventType is SimulationRoomEvent.Maintenance);

                    var simRoomEvent = new NetSimRoomEvent()
                    {
                        Location = new NetSimRoomEventLocationInfo { Chapter = simRoomChapter.Chapter, Stage = i, Order = 1 },
                    };

                    if (simRoomSelectionEventRecord is not null)
                    {
                        simRoomEvent.Selection = new NetSimRoomSelectionEvent
                        {
                            Id = simRoomSelectionEventRecord.Id,
                            SelectedNumber = 1
                        };
                        var groupRecordsBySelectionGroupId = simRoomSelectionEventGroupRecords.FindAll(x => x.SelectionGroupId == simRoomSelectionEventRecord.SelectionGroupId);
                        foreach (var groupRecord in groupRecordsBySelectionGroupId)
                        {
                            simRoomEvent.Selection.Group.Add(new NetSimRoomSelectionGroupElement { SelectionNumber = groupRecord.SelectionNumber, Id = groupRecord.Id });
                        }
                    }

                    events.Add(NetToM(simRoomEvent));
                    netSimRooms.Add(simRoomEvent);
                    log.Debug($"stage: {i}, NetSimRoomEvent: {JsonConvert.SerializeObject(simRoomEvent)}");

                    // Random
                    var RandomSimRoomSelectionEventRecords = simRoomSelectionEventRecords.FindAll(x
                        => x.EventType is SimulationRoomEvent.RandomSelection or SimulationRoomEvent.EnhanceBuff);

                    var RandomSimRoomSelectionEventRecord = GetRandomItems(RandomSimRoomSelectionEventRecords, 1);

                    var RandomSimRoomEvent = new NetSimRoomEvent
                    {
                        Location = new NetSimRoomEventLocationInfo { Chapter = simRoomChapter.Chapter, Stage = i, Order = 2 },
                    };
                    if (RandomSimRoomSelectionEventRecord != null && RandomSimRoomSelectionEventRecord.Count >= 1)
                    {
                        RandomSimRoomEvent.Selection = new NetSimRoomSelectionEvent
                        {
                            Id = RandomSimRoomSelectionEventRecord[0].Id,
                            SelectedNumber = 2
                        };

                        var groupRecordsBySelectionGroupId = simRoomSelectionEventGroupRecords.FindAll(x
                            => x.SelectionGroupId == RandomSimRoomSelectionEventRecord[0].SelectionGroupId);

                        foreach (var groupRecord in groupRecordsBySelectionGroupId)
                        {
                            RandomSimRoomEvent.Selection.Group.Add(new NetSimRoomSelectionGroupElement { SelectionNumber = groupRecord.SelectionNumber, Id = groupRecord.Id });
                        }
                    }

                    events.Add(NetToM(RandomSimRoomEvent));
                    netSimRooms.Add(RandomSimRoomEvent);
                    log.Debug($"stage: {i}, NetSimRoomEvent: {JsonConvert.SerializeObject(RandomSimRoomEvent)}");
                }
                else
                {
                    var battleBuffPreviews = buffPreviewRecords.FindAll(x
                        => x.EventType is SimulationRoomEvent.NormalBattle or SimulationRoomEvent.EliteBattle);
                    var randomBuffPreview = GetRandomItems(battleBuffPreviews, 3);
                    int order = 0;
                    foreach (var simRoomBuffPreview in randomBuffPreview)
                    {
                        order += 1;
                        var simRoomBattleEvent = CreateSimRoomBattleEvent(chapter: simRoomChapter, stage: i, order: order, simRoomBattleEventRecords, simRoomBuffPreview);
                        events.Add(NetToM(simRoomBattleEvent));
                        netSimRooms.Add(simRoomBattleEvent);
                    }
                }
            }
            // user.AddTrigger()
            user.ResetableData.SimRoomData.Events = events;
            JsonDb.Save();
            return netSimRooms;
        }

        /// <summary>
        /// Get BuffOptions By User And SimRoomEventLocationInfo
        /// </summary>
        /// <param name="user"> User </param>
        /// <param name="location"> NetSimRoomEventLocationInfo </param>
        /// <returns>List<int></returns>
        public static List<int> GetBuffOptions(User user, NetSimRoomEventLocationInfo location)
        {
            var events = user.ResetableData.SimRoomData.Events;
            var simRoomEventIndex = events.FindIndex(x => x.Location.Chapter == location.Chapter && x.Location.Stage == location.Stage && x.Location.Order == location.Order);
            if (simRoomEventIndex > -1)
            {
                var simRoomEvent = events[simRoomEventIndex];
                if (GameData.Instance.SimulationRoomBuffPreviewTable.TryGetValue(simRoomEvent.Battle.BuffPreviewId, out var buffPreview))
                {
                    log.Debug($"SimRoomBuffPreview: {JsonConvert.SerializeObject(buffPreview)}");
                    List<SimulationRoomBuffRecord> buffRecords = [];
                    if (buffPreview.PreviewType is PreviewType.Bubble)
                    {
                        var bubbleType = GetBubbleType(buffPreview.PreviewTarget);
                        buffRecords = [.. GameData.Instance.SimulationRoomBuffTable.Values
                                .Where(x => x.BubbleType == bubbleType
                                && x.Grade is SimulationRoomBuffGrade.SR or SimulationRoomBuffGrade.SSR )];
                    }
                    else if (buffPreview.PreviewType is PreviewType.MainTarget)
                    {
                        var mainTarget = GetMainTarget(buffPreview.PreviewTarget);
                        buffRecords = [.. GameData.Instance.SimulationRoomBuffTable.Values
                                .Where(x => x.MainTarget == mainTarget
                                && x.Grade is SimulationRoomBuffGrade.SR or SimulationRoomBuffGrade.SSR)];
                    }
                    else if (buffPreview.PreviewType is PreviewType.Grade)
                    {
                        buffRecords = [.. GameData.Instance.SimulationRoomBuffTable.Values
                                .Where(x => x.Grade == SimulationRoomBuffGrade.EPIC)];
                    }
                    else
                    {
                        log.Warn($"Not Implement SimulationRoomBuffPreview.PreviewType: {buffPreview.PreviewType}");
                    }
                    if (buffRecords.Count > 0)
                    {
                        var selectedBuffs = buffRecords.GetRandomItems(3);
                        log.Debug($"Selected Buffs: {JsonConvert.SerializeObject(selectedBuffs)}");

                        // user SimRoomEvent update
                        UpdateUserSimRoomEvent(user, simRoomEventIndex, events, battleProgress: (int)SimRoomBattleEventProgress.RewardWaiting, BuffOptions: [.. selectedBuffs.Select(x => x.Id)]);
                        return [.. selectedBuffs.Select(x => x.Id)];
                    }
                    else
                    {
                        log.Warn($"Not Font SimulationRoomBuff");
                    }
                }
            }
            else
            {
                log.Warn($"Not Font User.ResetableData.SimRoomData.Events");
            }

            return [];
        }

        /// <summary>
        /// Get TeamData By User Or SimRoomCharacterHp
        /// </summary>
        /// <param name="user"> User </param>
        /// <param name="teamNumber"> Int </param>
        /// <param name="remainingHps"> List<NetSimRoomCharacterHp> </param>
        /// <returns> NetTeamData </returns>
        public static NetTeamData GetTeamData(User user, int teamNumber, List<NetSimRoomCharacterHp>? remainingHps)
        {
            try
            {
                if (remainingHps is not null && remainingHps.Count > 0)
                {
                    var team = new NetTeamData()
                    {
                        TeamNumber = teamNumber
                    };
                    int slot = 1;
                    foreach (var item in remainingHps)
                    {
                        team.Slots.Add(new NetTeamSlot() { Slot = slot, Value = item.Csn });
                        slot += 1;
                    }
                    return team;
                }
                else
                {
                    if (user.UserTeams.TryGetValue((int)TeamType.SimulationRoom, out var teamData))
                    {
                        var team = teamData.Teams.FirstOrDefault(t => t.TeamNumber == teamNumber);
                        if (team is not null) return team;
                    }
                }
            }
            catch (Exception e)
            {
                log.Error($"Get User Teams Exception: {e.Message}");
            }

            // default value is null
            return null;
        }

        /// <summary>
        /// Randomly select a specified number of elements from the list.
        /// </summary>
        public static List<T> GetRandomItems<T>(this List<T>? list, int count)
        {
            if (list is null || list.Count == 0)
                return [];

            // If the number of requests exceeds the list length, retrieve all.
            count = Math.Min(count, list.Count);

            return [.. list.OrderBy(x => _rand.Next()).Take(count)];
        }

        /// <summary>
        /// Create SimRoomBattleEvent
        /// </summary>
        /// <param name="chapter"> SimulationRoomChapterRecord </param>
        /// <param name="stage"></param>
        /// <param name="order"></param>
        /// <param name="simRoomBattleEventRecords"></param>
        /// <param name="randomBuffPreview"></param>
        /// <returns>NetSimRoomEvent</returns>
        private static NetSimRoomEvent CreateSimRoomBattleEvent(SimulationRoomChapterRecord chapter, int stage, int order,
            List<SimulationRoomBattleEventRecord> simRoomBattleEventRecords, SimulationRoomBuffPreviewRecord randomBuffPreview)
        {
            var simRoomEvent = new NetSimRoomEvent();
            var location = new NetSimRoomEventLocationInfo();
            var battle = new NetSimRoomBattleEvent();

            location.Chapter = chapter.Chapter;
            location.Stage = stage;
            location.Order = order;
            var simRoomBuffPreviewBattleEvents = simRoomBattleEventRecords.FindAll(x
                => x.EventType == randomBuffPreview.EventType && x.DifficultyConditionValue <= chapter.DifficultyId);
            log.Debug($"EventType: {randomBuffPreview.EventType}, SimRoomBattleEventRecord Count: {simRoomBuffPreviewBattleEvents.Count}");
            var randomBattleEvents = GetRandomItems(simRoomBuffPreviewBattleEvents, 1);
            foreach (var battleEvent in randomBattleEvents)
            {
                battle.Id = battleEvent.Id;
                battle.RemainingTargetHealth = 10000;
                battle.BuffPreviewId = randomBuffPreview.Id;
            }

            simRoomEvent.Location = location;
            simRoomEvent.Battle = battle;
            log.Debug($"stage: {stage}, NetSimRoomEvent: {JsonConvert.SerializeObject(simRoomEvent)}");
            return simRoomEvent;
        }

        public static NetSimRoomEvent MToNet(SimRoomEvent simRoomEvent)
        {
            var netSimRoomEvent = new NetSimRoomEvent
            {
                Selected = simRoomEvent.Selected,
            };
            if (simRoomEvent.Location is not null && simRoomEvent.Location.Chapter > 0)
            {
                netSimRoomEvent.Location = new NetSimRoomEventLocationInfo
                {
                    Chapter = simRoomEvent.Location.Chapter,
                    Stage = simRoomEvent.Location.Stage,
                    Order = simRoomEvent.Location.Order
                };
            }

            if (simRoomEvent.Battle is not null && simRoomEvent.Battle.Id > 0)
            {
                netSimRoomEvent.Battle = new NetSimRoomBattleEvent
                {
                    Id = simRoomEvent.Battle.Id,
                    BuffOptions = { simRoomEvent.Battle.BuffOptions },
                    Progress = (SimRoomBattleEventProgress)simRoomEvent.Battle.Progress,
                    RemainingTargetHealth = simRoomEvent.Battle.RemainingTargetHealth,
                    BuffPreviewId = simRoomEvent.Battle.BuffPreviewId,
                };
            }

            if (simRoomEvent.Selection is not null && simRoomEvent.Selection.Id > 0)
            {
                netSimRoomEvent.Selection = new NetSimRoomSelectionEvent
                {
                    Id = simRoomEvent.Selection.Id,
                    SelectedNumber = simRoomEvent.Selection.SelectedNumber,
                };
                simRoomEvent.Selection.Group.ForEach(g =>
                {
                    netSimRoomEvent.Selection.Group.Add(new NetSimRoomSelectionGroupElement
                    {
                        SelectionNumber = g.SelectionNumber,
                        Id = g.Id,
                        IsDone = g.IsDone,
                        RandomBuff = g.RandomBuff,
                    });
                });
            }
            return netSimRoomEvent;
        }

        public static SimRoomEvent NetToM(NetSimRoomEvent simRoomEvent)
        {
            var netSimRoomEvent = new SimRoomEvent
            {
                Selected = simRoomEvent.Selected,
                EventCase = (int)simRoomEvent.EventCase,
            };
            if (simRoomEvent.Location is not null && simRoomEvent.Location.Chapter > 0)
            {
                netSimRoomEvent.Location = new SimRoomEventLocationInfo
                {
                    Chapter = simRoomEvent.Location.Chapter,
                    Stage = simRoomEvent.Location.Stage,
                    Order = simRoomEvent.Location.Order
                };
            }

            if (simRoomEvent.Battle is not null && simRoomEvent.Battle.Id > 0)
            {
                netSimRoomEvent.Battle = new SimRoomBattleEvent
                {
                    Id = simRoomEvent.Battle.Id,
                    Progress = (int)simRoomEvent.Battle.Progress,
                    RemainingTargetHealth = simRoomEvent.Battle.RemainingTargetHealth,
                    BuffPreviewId = simRoomEvent.Battle.BuffPreviewId,
                };
                foreach (var item in simRoomEvent.Battle.BuffOptions)
                {
                    netSimRoomEvent.Battle.BuffOptions.Add(item);
                }
            }

            if (simRoomEvent.Selection is not null && simRoomEvent.Selection.Id > 0)
            {
                netSimRoomEvent.Selection = new SimRoomSelectionEvent
                {
                    Id = simRoomEvent.Selection.Id,
                    SelectedNumber = simRoomEvent.Selection.SelectedNumber,
                };
                foreach (var g in simRoomEvent.Selection.Group)
                {
                    netSimRoomEvent.Selection.Group.Add(new SimRoomSelectionGroupElement
                    {
                        SelectionNumber = g.SelectionNumber,
                        Id = g.Id,
                        IsDone = g.IsDone,
                        RandomBuff = g.RandomBuff,
                    });
                }
            }
            return netSimRoomEvent;
        }

        public static SimulationRoomBubbleType GetBubbleType(string previewTarget)
        {
            // Type_C
            switch (previewTarget)
            {
                case "Type_A":
                    return SimulationRoomBubbleType.TypeA;
                case "Type_B":
                    return SimulationRoomBubbleType.TypeB;
                case "Type_C":
                    return SimulationRoomBubbleType.TypeC;
                case "Type_D":
                    return SimulationRoomBubbleType.TypeD;
                default:
                    log.Warn("Unknown preview target: " + previewTarget);
                    return SimulationRoomBubbleType.TypeA;
            }
        }

        public static SimulationRoomBuffMainTarget GetMainTarget(string previewTarget)
        {
            // Shoot = 0, Attack = 1, Survive = 2
            switch (previewTarget)
            {
                case "Shoot":
                    return SimulationRoomBuffMainTarget.Shoot;
                case "Attack":
                    return SimulationRoomBuffMainTarget.Attack;
                case "Survive":
                    return SimulationRoomBuffMainTarget.Survive;
                default:
                    log.Warn("Unknown preview target: " + previewTarget);
                    return SimulationRoomBuffMainTarget.Attack;
            }
        }

        /// <summary>
        /// Update User SimRoomEvent Events
        /// </summary>
        public static void UpdateUserSimRoomEvent(User user, int index, List<SimRoomEvent> events, int selectionNumber = 0,
            bool isDone = false, int battleProgress = 0, List<int>? BuffOptions = null)
        {
            try
            {
                var simRoomEvent = events[index];
                // user SimRoomEvent update
                simRoomEvent.Selected = true;

                // Update Selection Group
                var groupIndex = simRoomEvent.Selection.Group.FindIndex(x => x.SelectionNumber == selectionNumber);
                if (groupIndex > -1 && isDone)
                {
                    var group = simRoomEvent.Selection.Group[groupIndex];
                    group.IsDone = isDone;
                    simRoomEvent.Selection.Group[groupIndex] = group;
                }
                else
                {
                    log.Warn("Not Fond SimRoomSelectionEvent.Group");
                }

                // Udapte Battle Progress
                if (battleProgress > 0) simRoomEvent.Battle.Progress = battleProgress;

                // Update BuffOptions
                if (BuffOptions is not null && BuffOptions.Count > 0)
                {
                    simRoomEvent.Battle.BuffOptions = BuffOptions;
                }

                events[index] = simRoomEvent;
                user.ResetableData.SimRoomData.Events = events;
                log.Debug($"UpdateUserSimRoomEvent After UserSimRoomEventData: {JsonConvert.SerializeObject(user.ResetableData.SimRoomData.Events)}");
            }
            catch (Exception e)
            {
                log.Error($"Update UserSimRoomEvent Events Exception: {e.Message}");
            }
        }

        public static List<SimRoomCharacterHp> UpdateUserRemainingHps(User user, List<NetSimRoomCharacterHp>? remainingHps = null, int teamNumber = 1, int HpValue = 10000)
        {
            var userRemainingHps = user.ResetableData.SimRoomData.RemainingHps;
            try
            {
                // req remainingHps
                if (remainingHps is not null && remainingHps.Count > 0)
                {
                    foreach (var characterHp in remainingHps)
                    {
                        var userCharacterHpIndex = userRemainingHps.FindIndex(x => x.Csn == characterHp.Csn);
                        if (userCharacterHpIndex > -1)
                        {
                            userRemainingHps[userCharacterHpIndex] = new SimRoomCharacterHp { Csn = characterHp.Csn, Hp = characterHp.Hp };
                        }
                        else
                        {
                            userRemainingHps.Add(new SimRoomCharacterHp { Csn = characterHp.Csn, Hp = characterHp.Hp });
                        }
                    }
                }

                // get user team 
                if (user.UserTeams.TryGetValue((int)TeamType.SimulationRoom, out var userTeam))
                {
                    var team = userTeam.Teams.FirstOrDefault(x => x.TeamNumber == teamNumber);
                    if (team is not null)
                    {
                        foreach (var slot in team.Slots)
                        {
                            if (userRemainingHps.FindIndex(x => x.Csn == slot.Value) < 0) userRemainingHps.Add(new SimRoomCharacterHp { Csn = slot.Value, Hp = HpValue });
                        }
                    }
                }
                // update user
                user.ResetableData.SimRoomData.RemainingHps = userRemainingHps;
                return userRemainingHps;
            }
            catch (Exception e)
            {
                log.Error($"Update UserRemainingHps Exception: {e.Message}");
            }
            return userRemainingHps;
        }


        public static NetRewardData? SimRoomReceivedReward(User user, int difficultyId, int chapterId)
        {
            // check if reward is received
            NetRewardData? reward = null;
            if (!IsRewardReceived(user, difficultyId, chapterId))
            {

                var allChapterRecords = GameData.Instance.SimulationRoomChapterTable.Values;
                var chapter = allChapterRecords
                    .FirstOrDefault(x => x.DifficultyId == difficultyId && x.Chapter == chapterId);

                if (chapter is not null && chapter.RewardId > 0)
                {
                    var receivedRewardChapters = user.ResetableData.SimRoomData.ReceivedRewardChapters;
                    var receivedRewardChapter = receivedRewardChapters
                        .OrderBy(x => x.Difficulty)
                        .ThenBy(x => x.Chapter)
                        .LastOrDefault();
                    SimulationRoomChapterRecord? receivedRewardChapterRecord = null;
                    if (receivedRewardChapter is not null)
                    {
                        receivedRewardChapterRecord = allChapterRecords
                            .FirstOrDefault(x => x.DifficultyId == receivedRewardChapter.Difficulty
                                              && x.Chapter == receivedRewardChapter.Chapter);
                    }

                    reward = new NetRewardData();
                    if (receivedRewardChapterRecord is null)
                    {
                        // Claiming the reward for the first time
                        reward = RewardUtils.RegisterRewardsForUser(user, rewardId: chapter.RewardId);
                        AddRewardChapter(user, difficultyId, chapterId);
                    }
                    // Check if the received reward chapter is lower than the current chapter
                    else if (receivedRewardChapterRecord.DifficultyId == difficultyId && receivedRewardChapterRecord.Chapter < chapter.Chapter)
                    {
                        // Claiming the reward for a higher chapter
                        reward = CalculateIncrementalReward(user, chapter, receivedRewardChapterRecord);
                        AddRewardChapter(user, difficultyId, chapterId);
                    }
                    // Check if the received reward chapter is lower than the current difficulty
                    else if (receivedRewardChapterRecord.DifficultyId < difficultyId)
                    {
                        // Claiming the reward for a higher difficulty
                        reward = RewardUtils.RegisterRewardsForUser(user, rewardId: chapter.RewardId);
                        AddRewardChapter(user, difficultyId, chapterId);
                    }
                }
            }
            return reward;
        }

        /// <summary>
        /// Check if reward is received
        /// </summary>
        /// <returns>True if reward is received, otherwise false</returns>
        private static bool IsRewardReceived(User user, int difficultyId, int chapterId)
        {
            var isReceived = user.ResetableData.SimRoomData.ReceivedRewardChapters.Any(x => x.Chapter == chapterId && x.Difficulty == difficultyId);
            log.Debug($"IsRewardReceived (diff: {difficultyId}, chapter: {chapterId}): {isReceived}");
            return isReceived;
        }
        /// <summary>
        /// Add reward chapter
        /// </summary>
        private static void AddRewardChapter(User user, int difficulty, int chapter)
        {
            user.ResetableData.SimRoomData.ReceivedRewardChapters.Add(new SimRoomChapterInfo()
            {
                Difficulty = difficulty,
                Chapter = chapter
            });
        }

        /// <summary>
        /// Calculate incremental reward
        /// </summary>
        /// <returns>Incremental reward data</returns>
        private static NetRewardData CalculateIncrementalReward(User user,
            SimulationRoomChapterRecord chapter, SimulationRoomChapterRecord receivedChapterRecord)
        {
            var reward = new NetRewardData();
            var rewardRecord = GameData.Instance.GetRewardTableEntry(chapter.RewardId);
            var receivedRewardRecord = GameData.Instance.GetRewardTableEntry(receivedChapterRecord.RewardId);

            if (rewardRecord?.Rewards == null || receivedRewardRecord?.Rewards == null)
            {
                // If rewardRecord or receivedRewardRecord is empty, return the complete reward record
                reward = RewardUtils.RegisterRewardsForUser(user, rewardId: chapter.RewardId);
            }
            else
            {
                var receivedRewardIds = receivedRewardRecord.Rewards
                    .Where(x => x != null)
                    .ToDictionary(x => x.RewardId, x => x.RewardValue);

                foreach (var rewardItem in rewardRecord.Rewards.Where(x => x != null))
                {
                    int receivedAmount = receivedRewardIds.GetValueOrDefault(rewardItem.RewardId, 0);
                    int remainingAmount = Math.Max(0, rewardItem.RewardValue - receivedAmount);

                    if (remainingAmount > 0)
                    {
                        RewardUtils.AddSingleObject(user, ref reward, rewardItem.RewardId,
                            rewardType: rewardItem.RewardType, remainingAmount);
                    }
                }
            }

            return reward;
        }
    }
}