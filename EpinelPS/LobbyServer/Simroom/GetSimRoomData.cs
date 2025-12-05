using Google.Protobuf.WellKnownTypes;
using EpinelPS.Utils;
using EpinelPS.Data;

namespace EpinelPS.LobbyServer.Simroom
{
    [PacketPath("/simroom/get")]
    public class GetSimRoomData : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            await ReadData<ReqGetSimRoom>();
            User user = GetUser();

            // ResGetSimRoom Fields 
            //   SimRoomStatus Status
            //   int CurrentDifficulty
            //   long NextRenewAt
            //   RepeatedField<NetSimRoomChapterInfo> ClearInfos
            //   RepeatedField<NetSimRoomEvent> Events
            //   RepeatedField<NetSimRoomCharacterHp> RemainingHps
            //   RepeatedField<int> Buffs
            //   RepeatedField<int> LegacyBuffs
            //   RepeatedField<int> OverclockOptionList
            //   NetSimRoomOverclockData OverclockData
            //   Timestamp NextLegacyBuffResetDate
            //   NetSimRoomSimpleModeBuffSelectionInfo NextSimpleModeBuffSelectionInfo
            //   NetSimRoomChapterInfo LastPlayedChapter
            //   bool IsSimpleModeSkipEnabled


            var CurrentDifficulty = user.ResetableData.SimRoomData.CurrentDifficulty;
            var currentChapter = user.ResetableData.SimRoomData.CurrentChapter;
            var buffs = user.ResetableData.SimRoomData.Buffs;
            var legacyBuffs = user.ResetableData.SimRoomData.LegacyBuffs;

            ResGetSimRoom response = new()
            {
                Status = SimRoomStatus.Ready,
                CurrentDifficulty = CurrentDifficulty,
                // NextRenewAt: Resets at 2 AM daily
                NextRenewAt = DateTimeHelper.GetNextDayAtTime("China Standard Time", 2).Ticks,
                // NextLegacyBuffResetDate: Resets at 2 AM every Tuesday
                NextLegacyBuffResetDate = DateTimeHelper.GetNextWeekdayAtTime("China Standard Time", DayOfWeek.Tuesday, 2).ToTimestamp(),
                IsSimpleModeSkipEnabled = user.ResetableData.SimRoomData.IsSimpleModeSkipEnabled,
                LastPlayedChapter = new NetSimRoomChapterInfo()
                {
                    Chapter = currentChapter,
                    Difficulty = CurrentDifficulty,
                }
            };

            // LegacyBuffs
            response.LegacyBuffs.AddRange(legacyBuffs);

            // OverclockData
            try
            {
                response.OverclockData = GetOverclockData(user: user);
            }
            catch (Exception ex)
            {
                Logging.WriteLine($"Get OverclockData Exception: {ex.Message}", LogType.Error);
            }

            // ClearInfos
            response.ClearInfos.AddRange(GetClearInfos(user));

            // OverclockOptionList
            if (user.ResetableData.SimRoomData.CurrentSeasonData.IsOverclock)
            {
                response.OverclockOptionList.AddRange(user.ResetableData.SimRoomData.CurrentSeasonData.CurrentOptionList);
            }

            // check if user is in sim room
            if (user.ResetableData.SimRoomData.Entered)
            {
                response.Status = SimRoomStatus.Progress;
                response.Events.AddRange(SimRoomHelper.GetSimRoomEvents(user));

                // TODO: Get RemainingHps
                response.RemainingHps.AddRange(GetCharacterHp(user));

                // Buffs = Buffs + LegacyBuffs
                response.Buffs.AddRange(buffs);
                response.Buffs.AddRange(legacyBuffs);

                // response.NextSimpleModeBuffSelectionInfo = new()
                // {
                //     RemainingBuffSelectCount = 8 - response.Buffs.Count,
                //     BuffOptions = { user.ResetableData.SimRoomData.Buffs }
                // };

            }


            await WriteDataAsync(response);
        }

        /// <summary>
        /// Get clear infos
        /// </summary>
        /// <param name="user"></param>
        /// <returns>List of cleared chapters</returns>
        private static List<NetSimRoomChapterInfo> GetClearInfos(User user)
        {
            List<NetSimRoomChapterInfo> clearInfos = [];
            try
            {
                var receivedRewards = user.ResetableData.SimRoomData.ReceivedRewardChapters;
                if (receivedRewards.Count > 0)
                {
                    // Get the last received reward chapter
                    var lastReceivedReward = receivedRewards.OrderBy(x => x.Difficulty).ThenBy(x => x.Chapter).LastOrDefault();
                    if (lastReceivedReward is not null)
                    {
                        var CurrentDifficulty = lastReceivedReward.Difficulty;
                        var CurrentChapter = lastReceivedReward.Chapter;

                        // Get all chapters where difficulty is less than or equal to current difficulty
                        var ChapterRecords = GameData.Instance.SimulationRoomChapterTable.Values.Where(x => x.DifficultyId <= CurrentDifficulty).ToList();

                        foreach (var chapterRecord in ChapterRecords)
                        {
                            // check if chapter is less than or equal to current chapter
                            if (chapterRecord.DifficultyId == CurrentDifficulty && chapterRecord.Chapter <= CurrentChapter)
                            {
                                clearInfos.Add(new NetSimRoomChapterInfo()
                                {
                                    Chapter = chapterRecord.Chapter,
                                    Difficulty = chapterRecord.DifficultyId,
                                });
                            }
                            // check if difficulty is less than current difficulty
                            else if (chapterRecord.DifficultyId < CurrentDifficulty)
                            {
                                clearInfos.Add(new NetSimRoomChapterInfo()
                                {
                                    Chapter = chapterRecord.Chapter,
                                    Difficulty = chapterRecord.DifficultyId,
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Logging.WriteLine($"Get ClearInfos Exception: {e.Message}", LogType.Error);
            }
            return clearInfos;
        }

        public static List<NetSimRoomCharacterHp> GetCharacterHp(User user)
        {
            List<NetSimRoomCharacterHp> hps = [];
            var userRemainingHps = user.ResetableData.SimRoomData.RemainingHps;
            foreach (var userRemainingHp in userRemainingHps)
            {
                hps.Add(new() { Csn = userRemainingHp.Csn, Hp = userRemainingHp.Hp });
            }
            return hps;
        }

        private static NetSimRoomOverclockData GetOverclockData(User user)
        {
            var currentSeasonData = user.ResetableData.SimRoomData.CurrentSeasonData;

            var netOverclockData = new NetSimRoomOverclockData
            {
                HasClearedLevel50 = currentSeasonData.HasClearedLevel50,
                WasInfinitePopupChecked = currentSeasonData.WasInfinitePopupChecked,
                WasMainSeasonResetPopupChecked = currentSeasonData.WasMainSeasonResetPopupChecked,
                WasSubSeasonResetPopupChecked = currentSeasonData.WasSubSeasonResetPopupChecked,

                // CurrentSeasonData
                CurrentSeasonData = new NetSimRoomOverclockSeasonData
                {
                    SeasonStartDate = DateTime.UtcNow.Date.AddDays(-2).ToTimestamp(),
                    SeasonEndDate = DateTime.UtcNow.Date.AddDays(12).ToTimestamp(),
                    IsSeasonOpen = true,
                    Season = currentSeasonData.CurrentSeason,
                    SubSeason = currentSeasonData.CurrentSubSeason,
                    SeasonWeekCount = 2,
                },

                // CurrentSeasonHighScore
                CurrentSeasonHighScore = new NetSimRoomOverclockHighScoreData
                {
                    CreatedAt = currentSeasonData.CurrentSeasonHighScore.CreatedAt ?? DateTime.UtcNow.Date.AddDays(-2).ToTimestamp(),
                    Season = currentSeasonData.CurrentSeasonHighScore.Season,
                    SubSeason = currentSeasonData.CurrentSeasonHighScore.SubSeason,
                    OptionList = { currentSeasonData.CurrentSeasonHighScore.OptionList },
                    OptionLevel = currentSeasonData.CurrentSeasonHighScore.OptionLevel,
                },
                // CurrentSubSeasonHighScore
                CurrentSubSeasonHighScore = new NetSimRoomOverclockHighScoreData
                {
                    CreatedAt = currentSeasonData.CurrentSubSeasonHighScore.CreatedAt ?? DateTime.UtcNow.Date.AddDays(-2).ToTimestamp(),
                    Season = currentSeasonData.CurrentSubSeasonHighScore.Season,
                    SubSeason = currentSeasonData.CurrentSubSeasonHighScore.SubSeason,
                    OptionList = { currentSeasonData.CurrentSubSeasonHighScore.OptionList },
                    OptionLevel = currentSeasonData.CurrentSubSeasonHighScore.OptionLevel,
                },

                // LatestOption
                LatestOption = new NetSimRoomOverclockOptionSettingData
                {
                    Season = currentSeasonData.LatestOption.Season,
                    OptionList = { currentSeasonData.LatestOption.OptionList },
                }
            };
            netOverclockData.CurrentSeasonData.Season = GameData.Instance.SimulationRoomOcSeasonTable.Keys.Max();
            netOverclockData.CurrentSeasonData.SubSeason = 3;

            return netOverclockData;
        }
    }
}
