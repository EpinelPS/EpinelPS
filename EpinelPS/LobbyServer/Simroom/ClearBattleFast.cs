using EpinelPS.Database;
using EpinelPS.Utils;
using log4net;

namespace EpinelPS.LobbyServer.Simroom
{
    [PacketPath("/simroom/fastclearbattle")]
    public class ClearBattleFast : LobbyMsgHandler
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(ClearBattle));

        protected override async Task HandleAsync()
        {
            // {"location":{"chapter":3,"stage":3,"order":2},"event":111011143,"teamNumber":1,"antiCheatAdditionalInfo":{"clientLocalTime":"638993283799771900"}}
            ReqFastClearSimRoomBattle req = await ReadData<ReqFastClearSimRoomBattle>();
            User user = GetUser();

            ResFastClearSimRoomBattle response = new()
            {
                Result = SimRoomResult.Success
            };

            // OverclockOptionChangedHps 

            // Teams
            try
            {
                var team = SimRoomHelper.GetTeamData(user, req.TeamNumber, null);
                if (team is not null) response.Teams.Add(team);
            }
            catch (Exception e)
            {
                log.Error($"ClearBattleFast Response Team Exception :{e.Message}");
            }

            SimRoomHelper.UpdateUserRemainingHps(user, teamNumber: req.TeamNumber);

            // BuffOptions
            try
            {
                var buffOptions = SimRoomHelper.GetBuffOptions(user, req.Location);
                if (buffOptions is not null && buffOptions.Count > 0)
                {
                    response.BuffOptions.AddRange(buffOptions);
                }
            }
            catch (Exception e)
            {
                log.Error($"ClearBattleFast Response BuffOptions Exception: {e.Message}");
            }

            JsonDb.Save();
            await WriteDataAsync(response);
        }
    }
}