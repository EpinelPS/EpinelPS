using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Minigame.InTheMirror
{
    [PacketPath("/arcade/mvg/log/interaction")]
    public class GetMvgLogInteraction : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            await ReadData<ReqLogArcadeMvgInteraction>();

            await WriteDataAsync(new ResLogArcadeMvgInteraction());

        }
    }
}
