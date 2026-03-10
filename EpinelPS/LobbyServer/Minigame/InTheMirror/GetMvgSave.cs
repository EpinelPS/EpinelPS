using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Minigame.InTheMirror
{
    [PacketPath("/arcade/mvg/save")]
    public class GetMvgSave : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var request = await ReadData<ReqSaveArcadeMvgData>();

            var user = User;

            user.ArcadeInTheMirrorData = request.Data;

            await WriteDataAsync(new ResSaveArcadeMvgData());

            JsonDb.Save();
        }
    }
}
