using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Lostsector
{
    [PacketPath("/lostsector/fastclearstage")]
    public class QuickClearStage : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqFastClearLostSectorStage req = await ReadData<ReqFastClearLostSectorStage>();
            User user = GetUser();

            ResFastClearLostSectorStage response = new();

            ClearStage.ClearLostSectorStage(user, req.StageId);
            JsonDb.Save();

            await WriteDataAsync(response);
        }

    }
}
