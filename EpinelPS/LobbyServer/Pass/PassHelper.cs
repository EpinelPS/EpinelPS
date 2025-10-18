using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Utils;
using log4net;
using Newtonsoft.Json;

namespace EpinelPS.LobbyServer.Pass
{
    public static class PassHelper
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(PassHelper));

        public static NetPassInfo GetPassInfo(User user, int passId, int passPointId)
        {
            NetPassInfo passInfo = new();
            try
            {
                var userPass = user.UserPassInfo.GetValueOrDefault(passId);
                // If user does not have this pass info, create a new one
                if (userPass == null || userPass.PassId == 0)
                {
                    userPass = new PassData
                    {
                        PassId = passId,
                        PremiumActive = true,
                        PassPoint = 0,
                        PassRankList = [],
                        PassMissionList = []
                    };
                }
                log.Debug($"UserPassInfo before completing missions: {JsonConvert.SerializeObject(userPass)}");
                passInfo.PassId = userPass.PassId;
                passInfo.PremiumActive = userPass.PremiumActive;

                // Populate PassRankList based on SeasonPassTable
                var seasonPass = GameData.Instance.SeasonPassTable.Values.Where(sp => sp.PassId == passInfo.PassId).ToList();
                if (seasonPass != null && seasonPass.Count > 0)
                {
                    passInfo.PassPoint = userPass.PassPoint;
                    foreach (var sp in seasonPass)
                    {
                        if (userPass.PassRankList.Any(pr => pr.PassRank == sp.PassRank))
                        {
                            // Use existing rank data
                            var existingRank = userPass.PassRankList.First(pr => pr.PassRank == sp.PassRank);
                            passInfo.PassRankList.Add(new NetPassRankData
                            {
                                PassRank = existingRank.PassRank,
                                IsNormalRewarded = existingRank.IsNormalRewarded,
                                IsPremiumRewarded = existingRank.IsPremiumRewarded
                            });
                        }
                        else
                        {
                            // If the user does not have this rank yet, add it as not rewarded
                            userPass.PassRankList.Add(new PassRankData { PassRank = sp.PassRank, IsNormalRewarded = false, IsPremiumRewarded = false });
                            passInfo.PassRankList.Add(new NetPassRankData { PassRank = sp.PassRank, IsNormalRewarded = false, IsPremiumRewarded = false });
                        }
                    }
                }

                // Populate PassMissionList based on PassMissionTable
                var passMissions = GameData.Instance.PassMissionTable.Values.Where(pm => pm.PassPointId == passPointId).ToList();
                if (passMissions != null && passMissions.Count > 0)
                {
                    foreach (var pm in passMissions)
                    {
                        if (userPass.PassMissionList.Any(m => m.PassMissionId == pm.Id))
                        {
                            // Use existing mission data
                            var existingMission = userPass.PassMissionList.First(m => m.PassMissionId == pm.Id);
                            existingMission.IsComplete = userPass.LastCompleteAt == DateTime.Now.ToString("yyyyMMdd") && existingMission.IsComplete;
                            passInfo.PassMissionList.Add(new NetPassMissionData
                            {
                                PassMissionId = existingMission.PassMissionId,
                                IsComplete = existingMission.IsComplete
                            });
                        }
                        else
                        {
                            // If the user does not have this mission yet, add it as not complete
                            userPass.PassMissionList.Add(new PassMissionData { PassMissionId = pm.Id, IsComplete = false });
                            passInfo.PassMissionList.Add(new NetPassMissionData { PassMissionId = pm.Id, IsComplete = false });
                        }
                    }
                }
                // Update user's pass info in database
                if (userPass != null && userPass.PassId != 0)
                {
                    if (!user.UserPassInfo.TryAdd(userPass.PassId, userPass))
                        user.UserPassInfo[userPass.PassId] = userPass;
                }
                log.Debug($"UserPassInfo after completing missions: {JsonConvert.SerializeObject(userPass)}");
                JsonDb.Save();
            }
            catch (Exception ex)
            {
                log.Error($"Error getting pass info for user {user.ID}, PassId: {passId}, PassPointId: {passPointId}, error: {ex.Message}");
            }
            return passInfo;
        }

        public static void RewardsForUser(User user, ref NetRewardData reward, int rewardId)
        {
            try
            {
                var rewardData = GameData.Instance.RewardDataRecords.GetValueOrDefault(rewardId);
                if (rewardData == null)
                {
                    log.Warn($"No reward data found for RewardId: {rewardId}");
                    return;
                }

                if (rewardData.UserExp != 0)
                {
                    int newXp = rewardData.UserExp + user.userPointData.ExperiencePoint;

                    int newLevelExp = GameData.Instance.GetUserMinXpForLevel(user.userPointData.UserLevel);
                    int newLevel = user.userPointData.UserLevel;

                    if (newLevelExp == -1)
                    {
                        log.Warn("Unknown user level value for xp " + newXp);
                    }

                    int newGems = 0;

                    while (newXp >= newLevelExp)
                    {
                        newLevel++;
                        newGems += 30;
                        newXp -= newLevelExp;
                        if (user.Currency.ContainsKey(CurrencyType.FreeCash))
                            user.Currency[CurrencyType.FreeCash] += 30;
                        else
                            user.Currency.Add(CurrencyType.FreeCash, 30);

                        newLevelExp = GameData.Instance.GetUserMinXpForLevel(newLevel);
                    }

                    // TODO: what is the difference between IncreaseExp and GainExp
                    // NOTE: Current Exp/Lv refers to after XP was added.

                    reward.UserExp = new NetIncreaseExpData()
                    {
                        BeforeExp = user.userPointData.ExperiencePoint,
                        BeforeLv = user.userPointData.UserLevel,

                        // IncreaseExp = rewardData.UserExp,
                        CurrentExp = newXp,
                        CurrentLv = newLevel,
                        GainExp = rewardData.UserExp,
                    };
                    user.userPointData.ExperiencePoint = newXp;
                    user.userPointData.UserLevel = newLevel;
                }

                foreach (var item in rewardData.Rewards)
                {
                    if (item.RewardType != RewardType.None)
                    {
                        RewardUtils.AddSingleObject(user, ref reward, item.RewardId, item.RewardType, item.RewardValue);
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error($"Error processing rewards for user {user.ID} with RewardId {rewardId}, error: {ex.Message}");
            }
        }

        public static void UpdateUserPassInfoRank(User user, int passId, List<int> rankList, bool IsNormalRewarded, bool IsPremiumRewarded, int key = 0)
        {
            try
            {
                if (user.UserPassInfo.TryGetValue(passId, out PassData? passData))
                {
                    foreach (var rank in rankList)
                    {
                        if (passData.PassRankList.Any(pr => pr.PassRank == rank))
                        {
                            var existingRank = passData.PassRankList.First(pr => pr.PassRank == rank);
                            if (key == 0 || key == 1) existingRank.IsNormalRewarded = IsNormalRewarded;
                            if (key == 0 || key == 2) existingRank.IsPremiumRewarded = IsPremiumRewarded;
                        }
                        else
                        {
                            // If the user does not have this rank yet, add it
                            passData.PassRankList.Add(new PassRankData
                            {
                                PassRank = rank,
                                IsNormalRewarded = IsNormalRewarded,
                                IsPremiumRewarded = IsPremiumRewarded
                            });
                        }
                        log.Debug($"Updated pass rank info for user {user.ID} with PassId: {passId}, Ranks: {string.Join(", ", rankList)} userPassInfo: {JsonConvert.SerializeObject(passData)}");
                    }
                    JsonDb.Save();
                }
                else
                {
                    Logging.WriteLine($"No pass data found for user {user.ID} with PassId: {passId}");
                }
            }
            catch (Exception ex)
            {
                Logging.WriteLine($"Error updating pass rank info for user {user.ID}, PassId: {passId}, error: {ex.Message}");
            }
        }

        public static void ObtainPassRewards(User user, ref NetRewardData reward, int passId, List<int> rankList)
        {
            try
            {
                var seasons = GameData.Instance.SeasonPassTable.Values.Where(sp => sp.PassId == passId && rankList.Contains(sp.PassRank)).ToList();
                if (seasons.Count == 0)
                {
                    Logging.WriteLine($"No such pass id: {passId} or ranks: {string.Join(", ", rankList)}");
                }
                else
                {
                    foreach (var season in seasons)
                    {
                        bool isFreeRewarded = false;
                        bool isPremiumRewarded = false;
                        // check if the user has already claimed these rewards
                        if (user.UserPassInfo.TryGetValue(passId, out PassData? passData))
                        {
                            var existingRank = passData.PassRankList.FirstOrDefault(pr => pr.PassRank == season.PassRank);
                            if (existingRank != null)
                            {
                                isFreeRewarded = existingRank.IsNormalRewarded;
                                isPremiumRewarded = existingRank.IsPremiumRewarded;
                            }
                        }
                        // give rewards if not already claimed
                        if (season.FreeReward != 0 && !isFreeRewarded)
                            RewardsForUser(user, ref reward, season.FreeReward);
                        if (season.PremiumReward1 != 0 && !isPremiumRewarded)
                            RewardsForUser(user, ref reward, season.PremiumReward1);
                        if (season.PremiumReward2 != 0 && !isPremiumRewarded)
                            RewardsForUser(user, ref reward, season.PremiumReward2);
                    }
                    // update user pass info to mark these ranks as rewarded
                    // No abnormal judgment was made here,
                    UpdateUserPassInfoRank(user, passId, rankList, true, true);
                }
            }
            catch (Exception ex)
            {
                Logging.WriteLine($"Error obtaining pass rewards for user {user.ID}, PassId: {passId}, Ranks: {string.Join(", ", rankList)}, error: {ex.Message}");
            }
        }

        public static void ObtainOnePassRewards(User user, ref NetRewardData reward, int passId, int rank, bool premiumReward)
        {
            try
            {
                SeasonPassRecord? season = GameData.Instance.SeasonPassTable.Values.Where(sp => sp.PassId == passId && sp.PassRank == rank).FirstOrDefault();
                if (season == null || season.PassId == 0)
                {
                    Logging.WriteLine($"No such pass id: {passId} or rank: {rank}");
                }
                else
                {
                    int key = premiumReward ? 2 : 1;
                    if (!premiumReward && season.FreeReward != 0)
                        RewardsForUser(user, ref reward, season.FreeReward);
                    if (premiumReward && season.PremiumReward1 != 0)
                        RewardsForUser(user, ref reward, season.PremiumReward1);
                    if (premiumReward && season.PremiumReward2 != 0)
                        RewardsForUser(user, ref reward, season.PremiumReward2);
                    // update user pass info to mark these ranks as rewarded
                    // No abnormal judgment was made here,
                    UpdateUserPassInfoRank(user, passId, [season.PassRank], !premiumReward, premiumReward, key);
                }
            }
            catch (Exception ex)
            {
                Logging.WriteLine($"Error obtaining pass rewards for user {user.ID}, PassId: {passId}, Rank: {rank}, error: {ex.Message}");
            }
        }

        public static void BuyRank(User user, int passId, int targetPassRank, out int passPoint, out NetUserCurrencyData currencie)
        {
            int rankPrice = 200; // each rank costs 200 currency units
            passPoint = 0;
            currencie = new();

            if (user.Currency.TryGetValue(CurrencyType.ChargeCash, out long value) && value < rankPrice)
            {
                Logging.WriteLine($"User {user.ID} does not have enough ChargeCash to buy rank. Required: {rankPrice}, Available: {value}");
                currencie = new() { Type = (int)CurrencyType.ChargeCash, Value = value };
                return;
            }

            SeasonPassRecord? season = GameData.Instance.SeasonPassTable.Values.Where(sp => sp.PassId == passId && sp.PassRank == targetPassRank).FirstOrDefault();
            if (season == null)
            {
                Logging.WriteLine($"No such pass id: {passId} or rank: {targetPassRank}");
                return;
            }

            user.Currency[CurrencyType.ChargeCash] -= rankPrice;
            passPoint = season.ConditionValue;
            currencie = new() { Type = (int)CurrencyType.ChargeCash, Value = user.Currency[CurrencyType.ChargeCash] };
            if (user.UserPassInfo.TryGetValue(passId, out PassData? passData))
            {
                passData.PassPoint = passPoint;
            }
            else
            {
                passData = new PassData
                {
                    PassId = passId,
                    PremiumActive = true,
                    PassPoint = season.ConditionValue,
                    PassRankList = [],
                    PassMissionList = []
                };
                user.UserPassInfo.Add(passId, passData);
            }
            JsonDb.Save();
        }

        public static void CompletePassMissions(User user, ref NetRewardData reward, int passId, List<int> missionIds)
        {
            if (!user.UserPassInfo.TryGetValue(passId, out var passData))
            {
                passData = new();
                user.UserPassInfo.Add(passId, passData);
            }
            log.Debug($"UserPassInfo before completing missions: {JsonConvert.SerializeObject(passData)}");
            int completedPoints = 0;
            var completedMissions = GameData.Instance.PassMissionTable.Values
                .Where(pm => missionIds.Contains(pm.Id)).ToList();
            if (completedMissions.Count == 0)
            {
                log.Warn($"User {user.ID} has no valid pass missions to complete for pass {passId}");
                return;
            }
            foreach (var mission in completedMissions)
            {
                var existingMission = passData.PassMissionList.FirstOrDefault(m => m.PassMissionId == mission.Id);
                if (existingMission != null && existingMission.IsComplete)
                {
                    log.Warn($"User {user.ID} has already completed pass mission {mission.Id} for pass {passId}");  
                    continue;
                }
                var rewardEntry = GameData.Instance.GetRewardTableEntry(mission.RewardId);
                if (rewardEntry == null || rewardEntry.Rewards.Count == 0)
                {
                    log.Warn($"Unable to find reward entry {mission.RewardId} for pass mission {mission.Id}");
                    continue;
                }
                foreach (var rewardItem in rewardEntry.Rewards)
                {
                    if (rewardItem.RewardType == RewardType.PassPoint)
                    {
                        completedPoints += rewardItem.RewardValue;
                        existingMission.IsComplete = true;
                        passData.LastCompleteAt = DateTime.Now.ToString("yyyyMMdd");
                    }
                    else
                    {
                        log.Warn($"Unsupported reward type {rewardItem.RewardType} in pass mission {mission.Id}");
                    }
                }
            }
            user.AddTrigger(Trigger.PointRewardEvent, completedPoints, passId);
            passData.PassPoint += completedPoints;
            log.Debug($"UserPassInfo after completing missions: {JsonConvert.SerializeObject(passData)}");
            reward.PassPoint = new()
            {
                Value = completedPoints,
                FinalValue = passData.PassPoint
            };
            JsonDb.Save(); // Save user data after updating pass info
        }
    }
}