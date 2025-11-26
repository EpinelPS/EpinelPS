using EpinelPS.Database;
using EpinelPS.Utils;
using log4net;

namespace EpinelPS.LobbyServer.Simroom
{
    [PacketPath("/simroom/clearbattle")]
    public class ClearBattle : LobbyMsgHandler
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(ClearBattle));

        protected override async Task HandleAsync()
        {
            // {"location":{"chapter":3,"stage":3,"order":2},"event":111011143,"teamNumber":1,"antiCheatAdditionalInfo":{"clientLocalTime":"638993283799771900"}}
            ReqClearSimRoomBattle req = await ReadData<ReqClearSimRoomBattle>();
            User user = GetUser();

            ResClearSimRoomBattle response = new()
            {
                Result = SimRoomResult.Success
            };

            // OverclockOptionChangedHps

            // Teams
            try
            {
                var team = SimRoomHelper.GetTeamData(user, req.TeamNumber, [.. req.RemainingHps]);
                if (team is not null) response.Teams.Add(team);
            }
            catch (Exception e)
            {
                log.Error($"ClearBattle Response Team Exception :{e.Message}");
            }

            SimRoomHelper.UpdateUserRemainingHps(user, [.. req.RemainingHps], req.TeamNumber);

            if (req.BattleResult == 1)
            {
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
                    log.Error($"ClearBattle Response BuffOptions Exception :{e.Message}");
                }
            }

            JsonDb.Save();
            await WriteDataAsync(response);
        }


    }
}