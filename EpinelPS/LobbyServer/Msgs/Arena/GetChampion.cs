using EpinelPS.Utils;
using Google.Protobuf.WellKnownTypes;

namespace EpinelPS.LobbyServer.Msgs.Arena
{
    [PacketPath("/arena/champion/get")]
    public class GetChampion : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqGetChampionArena>();

            var response = new ResGetChampionArena();
            response.Schedule = new NetChampionArenaSchedule();
           
            // TODO

            await WriteDataAsync(response);
        }
    }
}
