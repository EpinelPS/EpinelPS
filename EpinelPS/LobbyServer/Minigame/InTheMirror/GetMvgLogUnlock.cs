using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Minigame.InTheMirror
{
    [PacketPath("/arcade/mvg/log/unlock")]
    public class GetMvgLogUpdate : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            await ReadData<ReqUnlockArcadeMvg>();

            await WriteDataAsync(new ResUnlockArcadeMvg());
        }
    }
}
