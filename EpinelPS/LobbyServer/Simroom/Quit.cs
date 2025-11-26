using EpinelPS.Utils;
using EpinelPS.Database;

namespace EpinelPS.LobbyServer.Simroom
{
    [PacketPath("/simroom/quit")]
    public class Quit : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqQuitSimRoom req = await ReadData<ReqQuitSimRoom>();
            User user = GetUser();

            ResQuitSimRoom response = new()
            {
                Result = SimRoomResult.Success,
            };

            try
            {
                foreach (var item in req.BuffsToAdd)
                {
                    if (!user.ResetableData.SimRoomData.LegacyBuffs.Contains(item))
                        user.ResetableData.SimRoomData.LegacyBuffs.Add(item);
                }
            }
            catch (Exception e)
            {
                Logging.Warn($"QuitSimRoom BuffsToAdd Exception {e.Message}");
            }

            try
            {
                foreach (var item in req.BuffsToDelete)
                {
                    user.ResetableData.SimRoomData.LegacyBuffs.Remove(item);
                }
            }
            catch (Exception e)
            {
                Logging.Warn($"QuitSimRoom BuffsToDelete Exception {e.Message}");
            }

            user.ResetableData.SimRoomData.Entered = false;
            user.ResetableData.SimRoomData.Events = [];
            user.ResetableData.SimRoomData.RemainingHps = [];
            user.ResetableData.SimRoomData.Buffs = [];

            JsonDb.Save();

            await WriteDataAsync(response);
        }
    }
}