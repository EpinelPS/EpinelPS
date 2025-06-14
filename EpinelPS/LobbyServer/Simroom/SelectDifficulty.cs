using EpinelPS.Utils;
using EpinelPS.Database;

namespace EpinelPS.LobbyServer.Simroom
{
    [PacketPath("/simroom/selectdifficulty")]
    public class SelectDifficulty : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqSelectSimRoomDifficulty>();
            var user = GetUser();

            ResSelectSimRoomDifficulty response = new()
            {
                Result = SimRoomResult.SimRoomResultSuccess,
            };

            user.ResetableData.SimRoomData.Entered = true;
            user.ResetableData.SimRoomData.CurrentDifficulty = req.Difficulty;
            user.ResetableData.SimRoomData.CurrentChapter = req.StartingChapter;

            // TODO: generate buffs

            JsonDb.Save();

            await WriteDataAsync(response);
        }
    }
}