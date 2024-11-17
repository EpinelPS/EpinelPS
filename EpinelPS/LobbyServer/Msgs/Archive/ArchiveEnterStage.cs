using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Msgs.Archive
{
    [PacketPath("/archive/storydungeon/enterstage")]
    public class EnterArchiveStage : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqEnterArchiveStage>();// has fields EventId StageId TeamNumber
			var evid = req.EventId;
			
            var response = new ResEnterArchiveStage();

            await WriteDataAsync(response);
        }
    }
}
