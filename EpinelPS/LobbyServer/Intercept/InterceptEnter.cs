using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Intercept
{
    [PacketPath("/intercept/enter")]
    public class EnterInterceptData : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqEnterIntercept req = await ReadData<ReqEnterIntercept>();
            User user = GetUser();

            ResEnterIntercept response = new();

            user.AddTrigger(Data.TriggerType.InterceptStart, 1);

            await WriteDataAsync(response);
        }
    }
}
