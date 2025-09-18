using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Minigame.InTheMirror
{
    [PacketPath("/arcade/mvg/log/collectable")]
    public class GetAcquireMvgCollectable : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var request = await ReadData<ReqAcquireArcadeMvgCollectable>();

            var user = GetUser();

            user.ArcadeInTheMirrorData.Collectables.Add(request.CollectableId);

            await WriteDataAsync(new ResAcquireArcadeMvgCollectable());

            JsonDb.Save();
        }
    }
}