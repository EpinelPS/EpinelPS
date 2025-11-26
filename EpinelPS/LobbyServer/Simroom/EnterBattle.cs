using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Simroom
{
    [PacketPath("/simroom/enterbattle")]
    public class EnterBattle : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            await ReadData<ReqEnterSimRoomBattle>();

            ResEnterSimRoomBattle response = new()
            {
                Result = SimRoomResult.Success
            };

            await WriteDataAsync(response);
        }
    }
}