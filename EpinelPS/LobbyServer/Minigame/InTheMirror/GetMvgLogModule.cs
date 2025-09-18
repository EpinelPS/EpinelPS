using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Minigame.InTheMirror
{
    [PacketPath("/arcade/mvg/log/module")]
    public class GetMvgLogModule : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            await ReadData<ReqEquipArcadeMvgModule>();

            await WriteDataAsync(new ResEquipArcadeMvgModule());
        }
    }
}
