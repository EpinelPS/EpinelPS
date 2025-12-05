using EpinelPS.Utils;
using EpinelPS.Database;

namespace EpinelPS.LobbyServer.Simroom
{
    [PacketPath("/simroom/selectdifficulty")]
    public class SelectDifficulty : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqSelectSimRoomDifficulty req = await ReadData<ReqSelectSimRoomDifficulty>();
            User user = GetUser();

            ResSelectSimRoomDifficulty response = new()
            {
                Result = SimRoomResult.Success,
            };

            
            user.ResetableData.SimRoomData.Entered = true;
            user.ResetableData.SimRoomData.CurrentDifficulty = req.Difficulty;
            user.ResetableData.SimRoomData.CurrentChapter = req.StartingChapter;
            // Update season data
            bool isOverclock = req.OverclockOptionList is not null && req.OverclockOptionList.Count > 0 && req.OverclockSeason > 0 && req.OverclockSubSeason > 0;
            var currentSeasonData = user.ResetableData.SimRoomData.CurrentSeasonData;
            currentSeasonData.IsOverclock = isOverclock;
            if (isOverclock)
            { 
                currentSeasonData.CurrentSeason = req.OverclockSeason;
                currentSeasonData.CurrentSubSeason = req.OverclockSubSeason;
                currentSeasonData.CurrentOptionList = [.. req.OverclockOptionList];
            }
            user.ResetableData.SimRoomData.CurrentSeasonData = currentSeasonData;
            JsonDb.Save();

            List<NetSimRoomEvent> events = SimRoomHelper.GetSimRoomEvents(user, req.Difficulty, req.StartingChapter,
             [.. req.OverclockOptionList], req.OverclockSeason, req.OverclockSubSeason);
             
            user.ResetableData.SimRoomData.Events = [.. events.Select(SimRoomHelper.NetToM)];
            JsonDb.Save();
            
            response.Events.AddRange(events);

            await WriteDataAsync(response);
        }
    }
}