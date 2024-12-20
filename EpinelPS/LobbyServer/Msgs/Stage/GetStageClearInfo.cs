using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Msgs.Stage
{
    [PacketPath("/stageclearinfo/get")]
    public class GetStageClearInfo : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqGetStageClearInfo>();
            var response = new ResGetStageClearInfo();
            var user = GetUser();

            response.Historys.AddRange(user.StageClearHistorys);
            
            await WriteDataAsync(response);
        }
    }
}
