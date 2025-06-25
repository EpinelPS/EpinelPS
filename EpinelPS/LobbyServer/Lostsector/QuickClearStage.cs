using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Lostsector
{
    [PacketPath("/lostsector/fastclearstage")]
    public class QuickClearStage : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqFastClearLostSectorStage>();
            var user = GetUser();

            var response = new ResFastClearLostSectorStage();

            ClearStage.ClearLostSectorStage(user, req.StageId);
            JsonDb.Save();

            await WriteDataAsync(response);
        }

    }
}
