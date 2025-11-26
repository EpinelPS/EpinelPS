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

            ResGetSimRoom response = new()
            {
                Status = SimRoomStatus.Ready,
                CurrentDifficulty = CurrentDifficulty,
                // NextRenewAt: Resets at 2 AM daily
                NextRenewAt = DateTimeHelper.GetNextDayAtTime("China Standard Time", 2).Ticks,
                // NextLegacyBuffResetDate: Resets at 2 AM every Tuesday
                NextLegacyBuffResetDate = DateTimeHelper.GetNextWeekdayAtTime("China Standard Time", DayOfWeek.Tuesday, 2).ToTimestamp(),
                IsSimpleModeSkipEnabled = user.ResetableData.SimRoomData.IsSimpleModeSkipEnabled,

            };

            // LegacyBuffs
            response.LegacyBuffs.AddRange(user.ResetableData.SimRoomData.LegacyBuffs);

            // OverclockData
            response.OverclockData = GetOverclockData(user: user);

            // ClearInfos
            response.ClearInfos.AddRange(GetClearInfos(user));

            // OverclockOptionList
            // response.OverclockOptionList.Add([]);

            // check if user is in sim room
            if (user.ResetableData.SimRoomData.Entered)
            {
                response.Status = SimRoomStatus.Progress;
                response.Events.AddRange(SimRoomHelper.GetSimRoomEvents(user));

                // TODO: Get RemainingHps
                response.RemainingHps.AddRange(GetCharacterHp(user));

                response.LastPlayedChapter = new NetSimRoomChapterInfo()
                {
                    Chapter = currentChapter,
                    Difficulty = CurrentDifficulty,
                };

                // Buffs = Buffs + LegacyBuffs
                response.Buffs.AddRange(user.ResetableData.SimRoomData.Buffs);
                response.Buffs.AddRange(user.ResetableData.SimRoomData.LegacyBuffs);

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
            if (user.UserTeams.TryGetValue((int)TeamType.SimulationRoom, out var userTeamData))
            {
                if (userTeamData.Teams.Count > 0 && userTeamData.Teams[0].Slots.Count > 0)
                {
                    foreach (var slot in userTeamData.Teams[0].Slots)
                    {
                        hps.Add(new() { Csn = slot.Value, Hp = 100000 });
                    }
                }
            }
            return hps;
        }

        private static NetSimRoomOverclockData GetOverclockData(User user)
        {
            return new NetSimRoomOverclockData
            {
                CurrentSeasonData = new NetSimRoomOverclockSeasonData
                {
                    SeasonStartDate = Timestamp.FromDateTimeOffset(DateTime.UtcNow.Date.AddDays(-1)),
                    SeasonEndDate = Timestamp.FromDateTimeOffset(DateTime.UtcNow.Date.AddDays(7)),
                    IsSeasonOpen = true,
                    Season = 1,
                    SubSeason = 1,
                    SeasonWeekCount = 1
                },

                CurrentSeasonHighScore = new NetSimRoomOverclockHighScoreData
                {
                    CreatedAt = Timestamp.FromDateTimeOffset(DateTime.UtcNow.Date.AddDays(-1)),
                    OptionLevel = 1,
                    Season = 1,
                    SubSeason = 1,
                    OptionList = { 1 }
                },

                CurrentSubSeasonHighScore = new NetSimRoomOverclockHighScoreData
                {
                    CreatedAt = Timestamp.FromDateTimeOffset(DateTime.UtcNow.Date.AddDays(-1)),
                    OptionLevel = 1,
                    Season = 1,
                    SubSeason = 1,
                    OptionList = { 1 }
                },

                LatestOption = new NetSimRoomOverclockOptionSettingData
                {
                    Season = 1,
                    OptionList = { 1 }
                }
            };
        }
    }
}
