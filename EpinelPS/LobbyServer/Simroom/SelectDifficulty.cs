using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Simroom
{
    [PacketPath("/simroom/selectdifficulty")]
    public class SelectDifficulty : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqSelectSimRoomDifficulty>();

            ResSelectSimRoomDifficulty response = new ResSelectSimRoomDifficulty
            {
                Result = SimRoomResult.SimRoomResultSuccess,
            };

            await WriteDataAsync(response);
        }
    }
}