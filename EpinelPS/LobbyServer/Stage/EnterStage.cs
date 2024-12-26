using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Stage
{
    [PacketPath("/stage/enterstage")]
    public class EnterStage : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqEnterStage>();
            var user = GetUser();

            var response = new ResEnterStage();
            
            user.AddTrigger(StaticInfo.TriggerType.CampaignStart, 1);

            await WriteDataAsync(response);
        }
    }
}
