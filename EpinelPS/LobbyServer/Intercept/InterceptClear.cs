using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Intercept
{
    [PacketPath("/intercept/clear")]
    public class ClearInterceptData : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqClearIntercept>();
            var user = GetUser();

            if (user.ResetableData.InterceptionTickets == 0)
            {
                Logging.WriteLine("Attempted to clear interception when 0 tickets remain", LogType.WarningAntiCheat);
               
            }
         
            var sRes = InterceptionHelper.Clear(user, req.Intercept, req.InterceptId, req.Damage);

            user.ResetableData.InterceptionTickets--;
            var response = new ResClearIntercept
            {
                Intercept = req.Intercept,
                InterceptId = req.InterceptId,
                TicketCount = user.ResetableData.InterceptionTickets,
                MaxTicketCount = JsonDb.Instance.MaxInterceptionCount,
                NormalReward = sRes.NormalReward,
            };

            user.AddTrigger(Data.TriggerType.InterceptClear, 1);

            JsonDb.Save();

            await WriteDataAsync(response);
        }
    }
}
