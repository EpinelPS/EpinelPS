using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Msgs.Event
{
    [PacketPath("/eventfield/enter")]
    public class EnterEventField : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqEnterEventField>();
            var user = GetUser();

            var response = new ResEnterEventField();

            // TOOD

            response.Field = new();
            
            if (user.MapJson.TryGetValue(req.MapId, out string mapJson))
            {
                response.Json = mapJson;
            }

            await WriteDataAsync(response);
        }
    }
}
