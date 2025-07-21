using EpinelPS.Utils;
using Google.Protobuf.WellKnownTypes;

namespace EpinelPS.LobbyServer.Arena
{
    [PacketPath("/arena/champion/get")]
    public class GetChampion : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqGetChampionArena req = await ReadData<ReqGetChampionArena>();

            ResGetChampionArena response = new()
            {
                Schedule = new NetChampionArenaSchedule()
            };

            // TODO

            await WriteDataAsync(response);
        }
    }
}
