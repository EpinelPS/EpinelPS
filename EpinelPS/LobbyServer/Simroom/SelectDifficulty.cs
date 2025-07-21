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

            // TODO: generate buffs

            JsonDb.Save();

            await WriteDataAsync(response);
        }
    }
}