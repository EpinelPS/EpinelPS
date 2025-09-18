using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Minigame.InTheMirror
{
    [PacketPath("/arcade/mvg/log/module/limit")]
    public class GetMvgLogModuleLimit : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            await ReadData<ReqIncreaseArcadeMvgModuleLimit>();

            await WriteDataAsync(new ResIncreaseArcadeMvgModuleLimit());
        }
    }
}
