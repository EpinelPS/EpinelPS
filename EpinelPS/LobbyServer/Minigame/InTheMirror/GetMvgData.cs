using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Minigame.InTheMirror
{
    [PacketPath("/arcade/mvg/get")]
    public class GetMvgData : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            await ReadData<ReqGetArcadeMvgData>();

            var user = GetUser();

            await WriteDataAsync(new ResGetArcadeMvgData() { Data = user.ArcadeInTheMirrorData });

        }
    }
}
