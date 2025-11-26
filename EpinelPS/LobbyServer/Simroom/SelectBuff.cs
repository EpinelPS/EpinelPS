using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Simroom
{
    [PacketPath("/simroom/selectbuff")]
    public class SelectBuff : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            // {"location":{"chapter":3,"stage":4,"order":3},"event":111011152,"buffToAdd":1030504}
            ReqSelectSimRoomBuff req = await ReadData<ReqSelectSimRoomBuff>();
            User user = GetUser();

            ResSelectSimRoomBuff response = new()
            {
                Result = SimRoomResult.Success
            };

            // Update User SimRoomData Buffs
            try
            {
                var buffs = user.ResetableData.SimRoomData.Buffs;
                if (req.BuffToDelete > 0)
                {
                    if (buffs.Contains(req.BuffToDelete)) buffs.Remove(req.BuffToDelete);
                }

                if (req.BuffToAdd > 0)
                {
                    if (!buffs.Contains(req.BuffToDelete)) buffs.Add(req.BuffToAdd);
                }
                user.ResetableData.SimRoomData.Buffs = buffs;
            }
            catch (Exception e)
            {
                Logging.Warn($"Update User SimRoomData Buffs Exception: {e.Message}");
            }

            // NetRewardData Reward
            // NetRewardData RewardByRewardUpEvent
            // NetRewardData RewardByOverclock

            var location = req.Location;
            if (location is not null)
            {
                var events = user.ResetableData.SimRoomData.Events;
                var simRoomEventIndex = events.FindIndex(x => x.Location.Chapter == location.Chapter && x.Location.Stage == location.Stage && x.Location.Order == location.Order);
                if (simRoomEventIndex > -1)
                {
                    // Update User SimRoomData Events
                    SimRoomHelper.UpdateUserSimRoomEvent(user, simRoomEventIndex, events, battleProgress: (int)SimRoomBattleEventProgress.RewardReceived);

                    // Reward
                    var sorted = events.OrderBy(x => x.Location.Stage).ThenBy(x => x.Location.Order).ToList(); // Sort by Stage, Order
                    var last = sorted[^1]; // Get last event
                    if (last.Location.Chapter == location.Chapter && last.Location.Stage == location.Stage && last.Location.Order == location.Order)
                    {
                        var difficulty = user.ResetableData.SimRoomData.CurrentDifficulty;
                        var reward = SimRoomHelper.SimRoomReceivedReward(user, difficulty, location.Chapter);
                        if (reward is not null) response.Reward = reward;
                    }
                }

                JsonDb.Save();
            }

            await WriteDataAsync(response);
        }

        


    }
}