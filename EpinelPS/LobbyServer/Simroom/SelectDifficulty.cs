using Google.Protobuf.WellKnownTypes;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Simroom
{
    [PacketPath("/simroom/selectdifficulty")]
    public class SelectDifficulty : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqSelectSimRoomDifficulty>();

            ResSelectSimRoomDifficulty response = new();
            
            // TODO
            
            await WriteDataAsync(response);
        }
    }
}
