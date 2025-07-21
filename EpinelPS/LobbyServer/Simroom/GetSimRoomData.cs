using Google.Protobuf.WellKnownTypes;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Simroom
{
    [PacketPath("/simroom/get")]
    public class GetSimRoomData : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqGetSimRoom req = await ReadData<ReqGetSimRoom>();
            Database.User user = GetUser();

            ResGetSimRoom response = new()
            {
                OverclockData = new NetSimRoomOverclockData
                {
                    CurrentSeasonData = new NetSimRoomOverclockSeasonData
                    {
                        SeasonStartDate = Timestamp.FromDateTimeOffset(DateTime.UtcNow),
                        SeasonEndDate = Timestamp.FromDateTimeOffset(DateTime.UtcNow.AddDays(7)),
                        IsSeasonOpen = true,
                        Season = 1,
                        SubSeason = 1,
                        SeasonWeekCount = 1
                    },

                    CurrentSeasonHighScore = new NetSimRoomOverclockHighScoreData
                    {
                        CreatedAt = Timestamp.FromDateTimeOffset(DateTime.UtcNow),
                        OptionLevel = 1,
                        Season = 1,
                        SubSeason = 1,
                        OptionList = {}
                    },

                    CurrentSubSeasonHighScore = new NetSimRoomOverclockHighScoreData
                    {
                        CreatedAt = Timestamp.FromDateTimeOffset(DateTime.UtcNow),
                        OptionLevel = 1,
                        Season = 1,
                        SubSeason = 1,
                        OptionList = {}
                    },

                    LatestOption = new NetSimRoomOverclockOptionSettingData
                    {
                        Season = 1,
                        OptionList = {}
                    }
                },

                Status = SimRoomStatus.Ready,
                CurrentDifficulty = 1,
                NextRenewAt = DateTime.UtcNow.AddDays(7).Ticks,
                NextLegacyBuffResetDate = Timestamp.FromDateTimeOffset(DateTime.UtcNow.AddDays(7))
            };

            if (user.ResetableData.SimRoomData.Entered)
            {
                response.Status = SimRoomStatus.Progress;
                
                response.CurrentDifficulty = user.ResetableData.SimRoomData.CurrentDifficulty;
            }


            await WriteDataAsync(response);
        }
    }
}
