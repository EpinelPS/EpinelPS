using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Episode
{
    [PacketPath("/episode/mission/enter")]
    public class ListMission : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqListValidEpMission>();

            var response = new ResListValidEpMission();

            // TOOD

            await WriteDataAsync(response);
        }
    }
}
