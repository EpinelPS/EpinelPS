using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Minigame.InTheMirror
{
    [PacketPath("/arcade/mvg/log/state")]
    public class GetMvgLogState : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            await ReadData<ReqLogArcadeMvgState>();

            await WriteDataAsync(new ResLogArcadeMvgState());
        }
    }
}
