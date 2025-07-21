using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Stage
{
    [PacketPath("/stageclearinfo/get")]
    public class GetStageClearInfo : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqGetStageClearInfo req = await ReadData<ReqGetStageClearInfo>();
            ResGetStageClearInfo response = new();
            User user = GetUser();

            response.Historys.AddRange(user.StageClearHistorys);
            
            await WriteDataAsync(response);
        }
    }
}
