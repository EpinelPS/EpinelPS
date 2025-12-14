using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Soloraid;

[PacketPath("/soloraid/getranking")]
public class GetRanking : LobbyMsgHandler
{
    protected override async Task HandleAsync()
    {
        await ReadData<ReqGetSoloRaidRanking>();

        ResGetSoloRaidRanking response = new();

        // TODO

        await WriteDataAsync(response);
    }
}