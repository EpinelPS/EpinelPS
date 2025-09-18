using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Minigame.InTheMirror
{
    [PacketPath("/arcade/mvg/newgame")]
    public class GetStartMvgNewGame : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            await ReadData<ReqStartArcadeMvgNewGame>();

            var user = GetUser();

            user.ArcadeInTheMirrorData.Gold = 0;
            user.ArcadeInTheMirrorData.Core = 0;
            user.ArcadeInTheMirrorData.Quests.Clear();
            user.ArcadeInTheMirrorData.Collectables.Clear();
            user.ArcadeInTheMirrorData.ProgressJson = "";

            await WriteDataAsync(new ResStartArcadeMvgNewGame());

            JsonDb.Save();
        }
    }
}
