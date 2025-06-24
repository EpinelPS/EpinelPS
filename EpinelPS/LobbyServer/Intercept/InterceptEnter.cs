using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Intercept
{
    [PacketPath("/intercept/enter")]
    public class EnterInterceptData : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqEnterIntercept>();
            var user = GetUser();

            var response = new ResEnterIntercept();

            user.AddTrigger(Data.TriggerType.InterceptStart, 1);

            await WriteDataAsync(response);
        }
    }
}
