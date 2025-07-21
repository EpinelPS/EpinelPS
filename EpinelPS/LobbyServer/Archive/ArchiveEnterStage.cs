using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Archive
{
    [PacketPath("/archive/storydungeon/enterstage")]
    public class EnterArchiveStage : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqEnterArchiveStage req = await ReadData<ReqEnterArchiveStage>();// has fields EventId StageId TeamNumber
            int evid = req.EventId;

            ResEnterArchiveStage response = new();

            await WriteDataAsync(response);
        }
    }
}
